using Quick.Protocol.Utils;
using System.Buffers;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Text;
using Quick.Protocol.Streams;
using Quick.Utils;

namespace Quick.Protocol
{
    public abstract partial class QpChannel
    {
        private readonly Pipe sendPipe = new Pipe();
        private readonly Pipe sendRawPipe = new Pipe();

        /// <summary>
        /// 当发送出错时
        /// </summary>
        protected virtual void OnWriteError(Exception exception)
        {
            LastException = exception;
            Options.Logger?.Log("[WriteError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            OnDisconnect();
            Dispose();
        }

        //压缩相关变量
        private Pipe writeCompressPipe = null;

        private async Task writePackageBuffer(PipeReader currentReader, QpPackageType packageType,
            int packageBodyLength, bool ignoreCompressAndEncrypt = false)
        {
            var stream = QpPackageHandler_Stream;
            if (stream == null)
                throw new IOException("Not connected.");

            //不带包头的包体
            ReadOnlySequence<byte> packageBodyBuffer = ReadOnlySequence<byte>.Empty;
            ReadResult readRet;
            if (packageBodyLength > 0)
            {
                readRet = await currentReader.ReadAtLeastAsync(packageBodyLength);
                packageBodyBuffer = readRet.Buffer;
            }

            int packageTotalLength = PACKAGE_HEAD_LENGTH + packageBodyLength;

            if (Options.Logger is { LogPackage: true })
            {
                var sb = new StringBuilder();
                sb.Append($"{DateTime.Now}: [Send-Package]Type: {packageType}");
                if (packageBodyLength > 0)
                {
                    if (Options.Logger.LogContent)
                        sb.Append(", Content: " + Convert.ToHexString(packageBodyBuffer.ToArray()));
                    else
                        sb.Append(QpLogger.NOT_SHOW_CONTENT_MESSAGE);
                }

                Options.Logger.Log(sb.ToString());
            }

            //如果有包体，且启用了压缩或者加密
            if (packageBodyLength > 0 && !ignoreCompressAndEncrypt &&
                (Options.InternalCompress || Options.InternalEncrypt))
            {
                //如果压缩
                if (Options.InternalCompress)
                {
                    if (writeCompressPipe == null)
                        writeCompressPipe = new Pipe();
                    using (var inStream = new ReadOnlySequenceByteStream(packageBodyBuffer))
                    using (var outStream = new PipeWriterStream(writeCompressPipe.Writer, true))
                    {
                        using (var gzStream = new GZipStream(outStream, CompressionMode.Compress, true))
                        {
                            await inStream.CopyToAsync(gzStream).ConfigureAwait(false);
                        }

                        packageBodyLength = Convert.ToInt32(outStream.Length);
                        _ = writeCompressPipe.Writer.FlushAsync();
                    }

                    //压缩完成，释放资源
                    currentReader?.AdvanceTo(packageBodyBuffer.End);
                    readRet = await writeCompressPipe.Reader.ReadAtLeastAsync(packageBodyLength).ConfigureAwait(false);
                    packageBodyBuffer = readRet.Buffer;

                    //包总长度
                    packageTotalLength = PACKAGE_HEAD_LENGTH + packageBodyLength;
                    currentReader = writeCompressPipe.Reader;
                }

                //如果加密
                if (Options.InternalEncrypt)
                {
                    try
                    {
                        //开始加密
                        var ret = enc.TransformFinalBlock(packageBodyBuffer.ToArray(), 0,
                            (int)packageBodyBuffer.Length);
                        //加密完成，释放资源
                        currentReader?.AdvanceTo(packageBodyBuffer.End);

                        packageBodyBuffer = new ReadOnlySequence<byte>(ret);
                        packageBodyLength = ret.Length;

                        //包总长度
                        packageTotalLength = PACKAGE_HEAD_LENGTH + packageBodyLength;
                        currentReader = null;
                    }
                    catch (Exception ex)
                    {
                        throw new IOException("发送数据加密时出错", ex);
                    }
                }
            }

            //发送数据
            {
                var writer = sendRawPipe.Writer;
                var headMemory = writer.GetMemory(PACKAGE_HEAD_LENGTH);
                //包头
                writePackageTotalLengthToBuffer(headMemory, packageTotalLength);
                headMemory.Span[4] = (byte)packageType;
                writer.Advance(PACKAGE_HEAD_LENGTH);
                //包体
                if (packageBodyLength > 0)
                {
                    var bodyMemory = writer.GetMemory(packageBodyLength);
                    packageBodyBuffer.CopyTo(bodyMemory.Span);
                    writer.Advance(packageBodyLength);
                }

                _ = writer.FlushAsync();

                //发送
                var reader = sendRawPipe.Reader;
                var rawRet = await reader.ReadAtLeastAsync(packageTotalLength);
                using (var sequenceByteStream = new ReadOnlySequenceByteStream(rawRet.Buffer))
                    await sequenceByteStream.CopyToAsync(stream)
                        .WaitAsync(TimeSpan.FromMilliseconds(Options.InternalTransportTimeout))
                        .ConfigureAwait(false);
                if (Options.EnableNetstat)
                {
                    BytesSent += packageTotalLength;
                    if (BytesSent > LONG_HALF_MAX_VALUE)
                        BytesSent = 0;
                }

                if (Options.Logger != null && Options.Logger.LogRaw)
                {
                    var sb = new StringBuilder();
                    sb.Append($"{DateTime.Now}: [Send-Raw]Length: {packageTotalLength}");
                    if (Options.Logger.LogContent)
                        sb.Append(", Content: " + Convert.ToHexString(rawRet.Buffer.ToArray()));
                    else
                        sb.Append(QpLogger.NOT_SHOW_CONTENT_MESSAGE);
                    Options.Logger.Log(sb.ToString());
                }

                reader.AdvanceTo(rawRet.Buffer.End);
            }
            if (packageBodyLength > 0)
                currentReader?.AdvanceTo(packageBodyBuffer.End);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        private static void writePackageTotalLengthToBuffer(byte[] buffer, int offset, int packageTotalLength)
        {
            writePackageTotalLengthToBuffer(new Span<byte>(buffer, offset, sizeof(int)), packageTotalLength);
        }

        private static void writePackageTotalLengthToBuffer(Span<byte> span, int packageTotalLength)
        {
            BitConverter.TryWriteBytes(span, packageTotalLength);
            if (BitConverter.IsLittleEndian)
                span.Slice(0, sizeof(int)).Reverse();
        }

        private static void writePackageTotalLengthToBuffer(Memory<byte> memory, int packageTotalLength)
        {
            writePackageTotalLengthToBuffer(memory.Span, packageTotalLength);
        }

        private async Task UseSendPipe(QpPackageType packageType, Func<Pipe, Task<int>> packageBodyHandler = null,
            bool ignoreCompressAndEncrypt = false)
        {
            if (Options.EnableNetstat)
                Interlocked.Increment(ref PackageSendQueueCount);
            await sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var packageBodyLength = 0;
                if (packageBodyHandler != null)
                    packageBodyLength = await packageBodyHandler(sendPipe);
                await writePackageBuffer(sendPipe.Reader, packageType, packageBodyLength, ignoreCompressAndEncrypt);
            }
            catch (Exception ex)
            {
                OnWriteError(ex);
                throw new Exception("发送数据时出错", ex);
            }
            finally
            {
                sendLock.Release();
                if (Options.EnableNetstat)
                    Interlocked.Decrement(ref PackageSendQueueCount);
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        public async Task SendHeartbeatPackage()
        {
            await UseSendPipe(QpPackageType.Heartbeat);
        }

        public async Task SendNoticePackage(string noticePackageTypeName, string noticePackageContent)
        {
            await UseSendPipe(QpPackageType.Notice, async pipe =>
            {
                var writer = pipe.Writer;

                var typeName = noticePackageTypeName;
                var content = noticePackageContent;
                var bodyLength = 0;
                //写入类名和长度
                {
                    var typeNameByteLength = encoding.GetByteCount(typeName);
                    writer.GetSpan(1)[0] = Convert.ToByte(typeNameByteLength);
                    writer.Advance(1);
                    bodyLength += 1;

                    encoding.GetEncoder().GetBytes(typeName, writer.GetSpan(typeNameByteLength), true);
                    writer.Advance(typeNameByteLength);
                    bodyLength += typeNameByteLength;
                }
                //写入内容
                {
                    var charMemory = content.AsMemory();
                    while (charMemory.Length > 0)
                    {
                        var charCount = Math.Min(minimumBufferSize, charMemory.Length);
                        var tmpCharMemory = charMemory.Slice(0, charCount);
                        charMemory = charMemory.Slice(charCount);

                        var byteCount = encoding.GetByteCount(tmpCharMemory.Span);
                        encoding.GetEncoder().GetBytes(tmpCharMemory.Span, writer.GetSpan(byteCount), true);
                        writer.Advance(byteCount);
                        bodyLength += byteCount;
                    }
                }
                _ = writer.FlushAsync();
                if (Options.Logger != null && Options.Logger.LogNotice)
                    Options.Logger.Log("{0}: [Send-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, Options
                        .Logger.LogContent
                        ? content
                        : QpLogger.NOT_SHOW_CONTENT_MESSAGE);
                return bodyLength;
            });
        }

        public Task SendCommandRequestPackage(string commandId, string typeName, string content)
        {
            return SendCommandRequestPackage(commandId, typeName, content, false);
        }

        /// <summary>
        /// 发送命令请求包
        /// </summary>
        private async Task SendCommandRequestPackage(string commandId, string typeName, string content,
            bool ignoreCompressAndEncrypt)
        {
            await UseSendPipe(QpPackageType.CommandRequest, async pipe =>
            {
                var writer = pipe.Writer;
                var bodyLength = 0;
                //写入指令编号
                {
                    var commandIdLength = commandId.Length / 2;
                    ByteUtils.HexDecode(commandId, writer.GetMemory(commandIdLength));
                    writer.Advance(commandIdLength);
                    bodyLength += commandIdLength;
                }
                //写入类名和长度
                {
                    var typeNameByteLength = encoding.GetByteCount(typeName);
                    writer.GetSpan(1)[0] = Convert.ToByte(typeNameByteLength);
                    writer.Advance(1);
                    bodyLength += 1;

                    encoding.GetEncoder().GetBytes(typeName, writer.GetSpan(typeNameByteLength), true);
                    writer.Advance(typeNameByteLength);
                    bodyLength += typeNameByteLength;
                }
                //写入内容
                {
                    var contentLength = encoding.GetByteCount(content);
                    encoding.GetEncoder().GetBytes(content, writer.GetSpan(contentLength), true);
                    writer.Advance(contentLength);
                    bodyLength += contentLength;
                }
                _ = writer.FlushAsync();
                if (Options.Logger is { LogCommand: true })
                    Options.Logger.Log("{0}: [Send-CommandRequestPackage]CommandId:{1},Type:{2},Content:{3}",
                        DateTime.Now, commandId, typeName, Options
                            .Logger.LogContent
                            ? content
                            : QpLogger.NOT_SHOW_CONTENT_MESSAGE);

                return bodyLength;
            }, ignoreCompressAndEncrypt);
        }

        /// <summary>
        /// 发送命令响应包
        /// </summary>
        public async Task SendCommandResponsePackage(string commandId, byte code, string message, string typeName,
            string content)
        {
            await UseSendPipe(QpPackageType.CommandResponse, async pipe =>
            {
                var writer = pipe.Writer;
                var bodyLength = 0;
                //写入指令编号
                {
                    var commandIdLength = commandId.Length / 2;
                    ByteUtils.HexDecode(commandId, writer.GetMemory(commandIdLength));
                    writer.Advance(commandIdLength);
                    bodyLength += commandIdLength;
                }
                //写入返回码
                {
                    writer.GetSpan(1)[0] = code;
                    writer.Advance(1);
                    bodyLength += 1;
                }
                //如果是成功
                if (code == 0)
                {
                    //写入类名和长度
                    {
                        var typeNameByteLength = encoding.GetByteCount(typeName);
                        writer.GetSpan(1)[0] = Convert.ToByte(typeNameByteLength);
                        writer.Advance(1);
                        bodyLength += 1;

                        encoding.GetEncoder().GetBytes(typeName, writer.GetSpan(typeNameByteLength), true);
                        writer.Advance(typeNameByteLength);
                        bodyLength += typeNameByteLength;
                    }
                    //写入内容
                    {
                        var contentLength = encoding.GetByteCount(content);
                        encoding.GetEncoder().GetBytes(content, writer.GetSpan(contentLength), true);
                        writer.Advance(contentLength);
                        bodyLength += contentLength;
                    }

                    if (Options.Logger is { LogCommand: true })
                        Options.Logger.Log(
                            "{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Type:{3},Content:{4}",
                            DateTime.Now, commandId, code, typeName, Options
                                .Logger.LogContent
                                ? content
                                : QpLogger.NOT_SHOW_CONTENT_MESSAGE);
                }
                //如果是失败
                else
                {
                    //写入消息
                    {
                        var messageLength = encoding.GetByteCount(message);
                        encoding.GetEncoder().GetBytes(message, writer.GetSpan(messageLength), true);
                        writer.Advance(messageLength);
                        bodyLength += messageLength;
                    }
                    if (Options.Logger is { LogNotice: true })
                        Options.Logger.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Message:{3}",
                            DateTime.Now, commandId, code, message);
                }

                _ = writer.FlushAsync();
                return bodyLength;
            });
        }
    }
}
