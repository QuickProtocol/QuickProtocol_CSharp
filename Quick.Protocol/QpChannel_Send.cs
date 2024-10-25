using Quick.Protocol.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO.Pipelines;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nerdbank.Streams;

namespace Quick.Protocol
{
    public abstract partial class QpChannel
    {
        private readonly Pipe sendPipe = new Pipe();
        private readonly byte[] sendHeadBuffer = new byte[5];

        /// <summary>
        /// 当发送出错时
        /// </summary>
        protected virtual void OnWriteError(Exception exception)
        {
            LastException = exception;
            LogUtils.Log("[WriteError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            InitQpPackageHandler_Stream(null);
            Disconnect();
        }

        //压缩相关变量
        private Pipe writeCompressPipe = null;
        //加密相关变量
        Pipe encryptPipe = null;
        byte[] encryptBuffer1 = null;
        byte[] encryptBuffer2 = null;

        private async Task writePackageBuffer(PipeReader currentReader, QpPackageType packageType, int packageBodyLength, bool ignoreCompressAndEncrypt = false)
        {
            var stream = QpPackageHandler_Stream;

            var readRet = await currentReader.ReadAtLeastAsync(packageBodyLength);
            var packageBodyBuffer = readRet.Buffer;

            var packageTotalLength = 0;
            Memory<byte> packageHeadMemory = default;

            //如果压缩或者加密
            if (!ignoreCompressAndEncrypt && (options.InternalCompress || options.InternalEncrypt))
            {
                //如果压缩
                if (options.InternalCompress)
                {
                    if (writeCompressPipe == null)
                        writeCompressPipe = new Pipe();
                    using (var inStream = packageBodyBuffer.AsStream())
                    using (var outStream = writeCompressPipe.Writer.AsStream(true))
                    using (var gzStream = new GZipStream(outStream, CompressionMode.Compress, true))
                        await inStream.CopyToAsync(gzStream).ConfigureAwait(false);

                    currentReader.AdvanceTo(packageBodyBuffer.End);

                    readRet = await writeCompressPipe.Reader.ReadAsync().ConfigureAwait(false);
                    packageBodyBuffer = readRet.Buffer;

                    //包总长度
                    packageTotalLength += PACKAGE_TOTAL_LENGTH_LENGTH;
                    //准备包头
                    writePackageTotalLengthToBuffer(sendHeadBuffer, 0, packageTotalLength);
                    packageHeadMemory = new Memory<byte>(sendHeadBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH);
                    currentReader = writeCompressPipe.Reader;
                }
                //如果加密
                if (options.InternalEncrypt)
                {
                    try
                    {
                        //准备管道
                        if (encryptPipe == null)
                        {
                            encryptPipe = new Pipe();
                            encryptBuffer1 = new byte[enc.InputBlockSize];
                            encryptBuffer2 = new byte[enc.OutputBlockSize];
                        }
                        //开始加密
                        var toEncryptedBuffer = packageBodyBuffer;
                        var inLength = 0;
                        while (toEncryptedBuffer.Length > 0)
                        {
                            inLength = Math.Min(encryptBuffer1.Length, (int)toEncryptedBuffer.Length);
                            toEncryptedBuffer.Slice(0, inLength).CopyTo(encryptBuffer1);
                            toEncryptedBuffer = toEncryptedBuffer.Slice(inLength);
                            if (inLength < enc.InputBlockSize)
                            {
                                var v = enc.InputBlockSize - inLength;
                                Array.Fill(encryptBuffer1, (byte)v, inLength, v);
                            }
                            var outLength = enc.TransformBlock(encryptBuffer1, 0, Math.Max(inLength, enc.InputBlockSize), encryptBuffer2, 0);
                            encryptBuffer2.CopyTo(encryptPipe.Writer.GetMemory(outLength));
                            encryptPipe.Writer.Advance(outLength);

                            packageTotalLength += outLength;
                        }
                        {
                            var finnalInLength = 0;
                            if (inLength == enc.InputBlockSize)
                            {
                                encryptBuffer1[0] = (byte)enc.InputBlockSize;
                                finnalInLength = 1;
                            }
                            var finalData = dec.TransformFinalBlock(encryptBuffer1, 0, finnalInLength);
                            if (finalData.Length > 0)
                            {
                                finalData.CopyTo(encryptPipe.Writer.GetMemory(finalData.Length));
                                encryptPipe.Writer.Advance(finalData.Length);
                                packageTotalLength += finalData.Length;
                            }
                        }
                        _ = Task.Run(async () =>
                        {
                            await encryptPipe.Writer.FlushAsync().ConfigureAwait(false);
                        });

                        readRet = await encryptPipe.Reader.ReadAtLeastAsync(packageTotalLength).ConfigureAwait(false);
                        currentReader.AdvanceTo(packageBodyBuffer.End);

                        packageBodyBuffer = readRet.Buffer;
                        //包总长度
                        packageTotalLength += PACKAGE_TOTAL_LENGTH_LENGTH;
                        //准备包头
                        writePackageTotalLengthToBuffer(sendHeadBuffer, 0, packageTotalLength);
                        packageHeadMemory = new Memory<byte>(sendHeadBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH);
                        currentReader = encryptPipe.Reader;
                    }
                    catch (Exception ex)
                    {
                        throw new IOException("发送数据加密时出错", ex);
                    }
                }
            }
            else
            {
                //包总长度
                packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + Convert.ToInt32(packageBodyBuffer.Length);
                //准备包头
                writePackageTotalLengthToBuffer(sendHeadBuffer, 0, packageTotalLength);
                packageHeadMemory = new Memory<byte>(sendHeadBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH);
            }
            //写入包头
            await stream.WriteAsync(packageHeadMemory).ConfigureAwait(false);

            //如果有包内容，写入包内容
            if (packageBodyBuffer.Length > 0)
            {
                var writeTask = packageBodyBuffer.AsStream().CopyToAsync(stream);
                await writeTask
                    .WaitAsync(TimeSpan.FromMilliseconds(options.InternalTransportTimeout))
                    .ConfigureAwait(false);

                if (writeTask.IsCanceled)
                    return;
                if (writeTask.Exception != null)
                    throw new IOException("Write error from stream.", writeTask.Exception.InnerException);
            }

            if (options.EnableNetstat)
            {
                BytesSent += packageHeadMemory.Length + packageBodyBuffer.Length;
                if (BytesSent > LONG_HALF_MAX_VALUE)
                    BytesSent = 0;
            }
            if (LogUtils.LogPackage)
                LogUtils.Log(
                    "{0}: [Send-Package]Length:{1}，Type:{2}，Content:{3}",
                    DateTime.Now,
                    packageTotalLength,
                    (QpPackageType)packageType,
                    LogUtils.LogContent ?
                        BitConverter.ToString(sendHeadBuffer.Concat(packageBodyBuffer.ToArray()).ToArray())
                        : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
            currentReader.AdvanceTo(packageBodyBuffer.End);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        private static void writePackageTotalLengthToBuffer(byte[] buffer, int offset, int packageTotalLength)
        {
            BitConverter.TryWriteBytes(new Span<byte>(buffer, offset, sizeof(int)), packageTotalLength);
            //如果是小端字节序，则交换
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer, offset, sizeof(int));
        }

        private async Task UseSendPipe(Func<Pipe, Task> handler)
        {
            if (options.EnableNetstat)
                Interlocked.Increment(ref PackageSendQueueCount);
            await sendLock.WaitAsync(5000).ConfigureAwait(false);
            try
            {
                await handler(sendPipe);
            }
            catch (Exception ex)
            {
                OnWriteError(ex);
                throw new Exception("发送数据时出错", ex);
            }
            finally
            {
                sendLock.Release();
                if (options.EnableNetstat)
                    Interlocked.Decrement(ref PackageSendQueueCount);
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        public async Task SendHeartbeatPackage()
        {
            await UseSendPipe(async pipe =>
            {
                var writer = pipe.Writer;
                //写入包类型
                writer.GetSpan(1)[0] = (byte)QpPackageType.Heartbeat;
                writer.Advance(1);
                _ = writer.FlushAsync();
                await writePackageBuffer(pipe.Reader,
                        QpPackageType.Heartbeat,
                        1).ConfigureAwait(false);
            });
        }

        public async Task SendNoticePackage(string noticePackageTypeName, string noticePackageContent)
        {
            await UseSendPipe(async pipe =>
            {
                var writer = pipe.Writer;
                //写入包类型
                writer.GetSpan(1)[0] = (byte)QpPackageType.Notice;
                writer.Advance(1);

                var typeName = noticePackageTypeName;
                var content = noticePackageContent;
                var bodyLength = 1;
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
                if (LogUtils.LogNotice)
                    LogUtils.Log("{0}: [Send-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
                await writePackageBuffer(pipe.Reader,
                        QpPackageType.Heartbeat,
                        1).ConfigureAwait(false);
            });
        }

        public Task SendCommandRequestPackage(string commandId, string typeName, string content)
        {
            return SendCommandRequestPackage(commandId, typeName, content, false);
        }

        /// <summary>
        /// 发送命令请求包
        /// </summary>
        private async Task SendCommandRequestPackage(string commandId, string typeName, string content, bool ignoreCompressAndEncrypt)
        {
            await UseSendPipe(async pipe =>
            {
                var writer = pipe.Writer;
                //写入包类型
                writer.GetSpan(1)[0] = (byte)QpPackageType.CommandRequest;
                writer.Advance(1);

                var bodyLength = 1;
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

                if (LogUtils.LogCommand)
                    LogUtils.Log("{0}: [Send-CommandRequestPackage]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandId, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                await writePackageBuffer(pipe.Reader,
                        QpPackageType.Heartbeat,
                        1,
                        ignoreCompressAndEncrypt).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// 发送命令响应包
        /// </summary>
        public async Task SendCommandResponsePackage(string commandId, byte code, string message, string typeName, string content)
        {
            await UseSendPipe(async pipe =>
            {
                var writer = pipe.Writer;
                //写入包类型
                writer.GetSpan(1)[0] = (byte)QpPackageType.CommandResponse;
                writer.Advance(1);

                var bodyLength = 1;
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

                    if (LogUtils.LogCommand)
                        LogUtils.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Type:{3},Content:{4}", DateTime.Now, commandId, code, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

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
                    if (LogUtils.LogNotice)
                        LogUtils.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Message:{3}", DateTime.Now, commandId, code, message);
                }
                _ = writer.FlushAsync();
                await writePackageBuffer(pipe.Reader,
                        QpPackageType.Heartbeat,
                        1).ConfigureAwait(false);
            });
        }
    }
}
