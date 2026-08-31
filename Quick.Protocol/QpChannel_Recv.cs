using Quick.Protocol.Exceptions;
using System.Buffers;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Text;
using Quick.Protocol.Streams;
using Quick.Utils;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Quick.Protocol
{
    public abstract partial class QpChannel
    {
        /// <summary>
        /// 从流中读取返回0是否代表错误
        /// </summary>
        protected virtual bool ReadFromStreamReturnZeroMeansFault { get; } = true;
        private DateTime lastReadDataTime;
        /// <summary>
        /// 当读取出错时
        /// </summary>
        protected virtual void OnReadError(Exception exception)
        {
            LastException = exception;
            Options.Logger?.Log("[ReadError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            InitChannelStream(null);
            Disconnect();
        }

        private async Task CheckRecvTimeoutAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                var sp = DateTime.Now - lastReadDataTime;
                if (sp.TotalMilliseconds > TransportTimeout)
                    throw new TimeoutException();
            }
        }

        private async Task FillRecvPipeAsync(Stream stream, PipeWriter writer, CancellationToken token)
        {
            var readBufferMemory = new Memory<byte>(new byte[minimumBufferSize]);
            while (!token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(readBufferMemory, token);
                if (bytesRead < 0)
                    throw new EndOfStreamException();
                if (bytesRead == 0)
                {
                    if (ReadFromStreamReturnZeroMeansFault)
                        throw new EndOfStreamException();
                    await Task.Delay(100, token);
                    continue;
                }
                lastReadDataTime = DateTime.Now;
                if (Options.EnableNetstat)
                {
                    BytesReceived += bytesRead;
                    if (BytesReceived > LONG_HALF_MAX_VALUE)
                        BytesReceived = 0;
                }
                await writer.WriteAsync(readBufferMemory.Slice(0, bytesRead), token);
                await writer.FlushAsync(token);
            }
        }

        private async Task ReadRecvPipeAsync(PipeReader recvReader, CancellationToken token)
        {
            //包体长度
            int packageBodyLength;
            byte packageType;
            ReadOnlySequence<byte> packageBodyBuffer;

            //解密相关变量
            Pipe decryptPipe = null;
            //解压相关变量
            Pipe decompressPipe = null;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    //读取包
                    var currentReader = recvReader;
                    {
                        var readTask = currentReader.ReadAtLeastAsync(PACKAGE_HEAD_LENGTH, token);
                        var ret = await readTask.AsTask()
                            .WaitAsync(TimeSpan.FromMilliseconds(TransportTimeout), token);
                        if (ret.IsCanceled)
                            return;
                        if (ret.Buffer.Length < PACKAGE_HEAD_LENGTH)
                            throw new ProtocolException(ret.Buffer, $"包头读取错误！包头长度：{PACKAGE_HEAD_LENGTH}，读取数据长度：{ret.Buffer.Length}");
                        
                        var packageHeadBuffer = ret.Buffer.Slice(0, 5);

                        var packageTotalLength = 0;
                        if (packageHeadBuffer.IsSingleSegment)
                        {
                            var packageHeadSpan = packageHeadBuffer.FirstSpan;
                            packageTotalLength = BinaryPrimitives.ReadInt32BigEndian(packageHeadSpan);
                            packageType = packageHeadSpan[PACKAGE_TOTAL_LENGTH_LENGTH];
                        }
                        else
                        {
                            var numberSpan = MemoryMarshal.CreateSpan(ref packageTotalLength, 1);
                            var byteSpan = MemoryMarshal.AsBytes(numberSpan);
                            packageHeadBuffer.Slice(0, byteSpan.Length).CopyTo(byteSpan);
                            //如果是小端字节序，则交换
                            if (BitConverter.IsLittleEndian)
                                byteSpan.Reverse();
                            packageType = packageHeadBuffer.Slice(byteSpan.Length).FirstSpan[0];
                        }
                        if (packageTotalLength < PACKAGE_HEAD_LENGTH)
                            throw new ProtocolException(ret.Buffer, $"包长度[{packageTotalLength}]必须大于等于{PACKAGE_HEAD_LENGTH}！");
                        if (packageTotalLength > Options.MaxPackageSize)
                            throw new ProtocolException(ret.Buffer, $"包长度[{packageTotalLength}]大于最大包大小[{Options.MaxPackageSize}]");

                        packageBodyLength = packageTotalLength - PACKAGE_HEAD_LENGTH;
                        currentReader.AdvanceTo(ret.Buffer.Start);

                        //读取完整包
                        ret = await currentReader.ReadAtLeastAsync(packageTotalLength, token);
                        if (ret.IsCanceled)
                            return;
                        if (ret.Buffer.Length < packageTotalLength)
                            throw new ProtocolException(ret.Buffer, $"包读取错误！包总长度：{packageTotalLength}，读取数据长度：{ret.Buffer.Length}");
                        var packageBuffer = ret.Buffer.Slice(0, packageTotalLength);
                        if (Options.Logger is { LogRaw: true })
                        {
                            var sb = new StringBuilder();
                            sb.Append($"{DateTime.Now}: [Recv-Raw]Length: {packageBuffer.Length}");
                            if (Options.Logger.LogContent)
                                sb.Append(", Content: " + Convert.ToHexString(packageBuffer.ToArray()));
                            else
                                sb.Append(QpLogger.NOT_SHOW_CONTENT_MESSAGE);
                            Options.Logger.Log(sb.ToString());
                        }
                        packageBodyBuffer = packageBuffer.Slice(PACKAGE_HEAD_LENGTH);
                    }
                    //如果有包体，且启用了压缩或者加密
                    if (packageBodyLength > 0 && (EnableCompress || EnableEncrypt))
                    {
                        //如果设置了加密
                        if (EnableEncrypt)
                        {
                            //准备管道
                            //使用关闭背压的管道选项：本管道是「先整体解密写入、再整体读出」的暂存，
                            //若沿用默认 64KB 背压，FlushAsync 会先阻塞、而解除背压的读取在其后，形成死锁。
                            if (decryptPipe == null)
                                decryptPipe = new Pipe(STAGING_PIPE_OPTIONS);
                            packageBodyLength = 0;

                            try
                            {
                                //开始解密
                                using (var readMs = new ReadOnlySequenceByteStream(packageBodyBuffer))
                                using (var decryptStream = new CryptoStream(readMs, dec, CryptoStreamMode.Read))
                                    while (true)
                                    {
                                        var memory = decryptPipe.Writer.GetMemory(minimumBufferSize);
                                        var count = await decryptStream.ReadAsync(memory);
                                        if (count <= 0)
                                        {
                                            decryptPipe.Writer.Advance(0);
                                            break;
                                        }
                                        decryptPipe.Writer.Advance(count);
                                        packageBodyLength += count;
                                    }
                                await decryptPipe.Writer.FlushAsync();
                                var ret = await decryptPipe.Reader.ReadAtLeastAsync(packageBodyLength);
                                //解密完成，释放缓存
                                currentReader?.AdvanceTo(packageBodyBuffer.End);
                                packageBodyBuffer = ret.Buffer;
                                currentReader = decryptPipe.Reader;
                            }
                            catch (Exception ex)
                            {
                                throw new IOException("接收数据解密时出错", ex);
                            }
                        }

                        //如果设置了压缩
                        if (EnableCompress)
                        {
                            //准备管道
                            if (decompressPipe == null)
                                decompressPipe = new Pipe(STAGING_PIPE_OPTIONS);

                            packageBodyLength = 0;
                            //开始解压
                            using (var readMs = new ReadOnlySequenceByteStream(packageBodyBuffer))
                            using (var gzStream = new GZipStream(readMs, CompressionMode.Decompress, true))
                            {
                                while (true)
                                {
                                    var count = await gzStream.ReadAsync(decompressPipe.Writer.GetMemory(minimumBufferSize), token);
                                    if (count <= 0)
                                        break;
                                    decompressPipe.Writer.Advance(count);
                                    packageBodyLength += count;
                                }
                            }
                            await decompressPipe.Writer.FlushAsync();
                            var ret = await decompressPipe.Reader.ReadAtLeastAsync(packageBodyLength, token);
                            //解压完成，释放缓存
                            currentReader?.AdvanceTo(packageBodyBuffer.End);
                            packageBodyBuffer = ret.Buffer;
                            currentReader = decompressPipe.Reader;
                        }
                    }
                    await HandlePackage(packageType, packageBodyBuffer);
                    currentReader?.AdvanceTo(packageBodyBuffer.End);
                }
            }
            finally
            {
                if (decryptPipe != null)
                {
                    await decryptPipe.Writer.CompleteAsync();
                    await decryptPipe.Reader.CompleteAsync();
                }
                if (decompressPipe != null)
                {
                    await decompressPipe.Writer.CompleteAsync();
                    await decompressPipe.Reader.CompleteAsync();
                }
            }
        }

        protected void BeginReadPackage(CancellationToken token)
        {
            lastReadDataTime = DateTime.Now;
            var pipe = new Pipe();
            //检查接收超时
            _ = Task.Run(async () =>
            {
                try
                {
                    await CheckRecvTimeoutAsync(token);
                }
                catch (Exception ex)
                {
                    OnReadError(ex);
                }
            });
            //填充接收管道
            _ = Task.Run(async () =>
            {
                try
                {
                    await FillRecvPipeAsync(channelStream, pipe.Writer, token);
                }
                catch (Exception ex)
                {
                    OnReadError(ex);
                }
                finally
                {
                    await pipe.Writer.CompleteAsync();
                }
            });
            //从接收管道读取数据并解析
            _ = Task.Run(async () =>
            {
                try
                {
                    await ReadRecvPipeAsync(pipe.Reader, token);
                }
                catch (Exception ex)
                {
                    OnReadError(ex);
                }
                finally
                {
                    await pipe.Reader.CompleteAsync();
                }
            });
        }
    }
}
