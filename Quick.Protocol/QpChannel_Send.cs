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


        private Pipe writeCompressPipe = new Pipe();

        private async Task writePackageBuffer(Stream stream, QpPackageType packageType, ReadOnlySequence<byte> packageBodyBuffer, Action afterSendHandler)
        {
            Pipe currentPipe = null;
            var packageTotalLength = 0;
            Memory<byte> packageHeadMemory = default;

            //如果压缩或者加密
            if (options.InternalCompress || options.InternalEncrypt)
            {
                //如果压缩
                if (options.InternalCompress)
                {
                    using (var inStream = packageBodyBuffer.AsStream())
                    using (var outStream = writeCompressPipe.Writer.AsStream(true))
                    using (var gzStream = new GZipStream(outStream, CompressionMode.Compress, true))                    
                        await inStream.CopyToAsync(gzStream).ConfigureAwait(false);

                    currentPipe = writeCompressPipe;
                    var readRet = await writeCompressPipe.Reader.ReadAsync().ConfigureAwait(false);
                    packageBodyBuffer = readRet.Buffer;
                    var packageBodyLength = Convert.ToInt32(packageBodyBuffer.Length);
                    //包总长度
                    packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + packageBodyLength;
                    //准备包头
                    writePackageTotalLengthToBuffer(sendHeadBuffer, 0, packageTotalLength);
                    packageHeadMemory = new Memory<byte>(sendHeadBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH);
                }
                /*
                //如果加密
                if (options.InternalEncrypt)
                {
                    var retBuffer = enc.TransformFinalBlock(packageBuffer.Array, packageBuffer.Offset + PACKAGE_TOTAL_LENGTH_LENGTH, packageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH);
                    var packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + retBuffer.Length;
                    var currentBuffer = getFreeBuffer(packageBuffer.Array, sendBuffer, sendBuffer2);
                    //写入包长度
                    writePackageTotalLengthToBuffer(currentBuffer, 0, packageTotalLength);
                    Array.Copy(retBuffer, 0, currentBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, retBuffer.Length);
                    packageBuffer = new ArraySegment<byte>(currentBuffer, 0, packageTotalLength);
                }
                */
            }
            else
            {
                //包总长度
                packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + Convert.ToInt32(packageBodyBuffer.Length);
                //准备包头
                writePackageTotalLengthToBuffer(sendHeadBuffer, 0, packageTotalLength);
                packageHeadMemory = new Memory<byte>(sendHeadBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH);
            }
            //执行AfterSendHandler
            afterSendHandler?.Invoke();
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
            if (currentPipe != null)
                currentPipe.Reader.AdvanceTo(packageBodyBuffer.End);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        private static void writePackageTotalLengthToBuffer(byte[] buffer, int offset, int packageTotalLength)
        {
            BitConverter.TryWriteBytes(new Span<byte>(buffer, offset, sizeof(int)), packageTotalLength);
            //如果是小端字节序，则交换
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer, offset, sizeof(int));
        }

        private async Task writePackageAsync(Func<PipeWriter, Task<Tuple<QpPackageType, int>>> getPackagePayloadFunc, Action afterSendHandler)
        {
            try
            {
                if (options.EnableNetstat)
                    Interlocked.Increment(ref PackageSendQueueCount);
                await sendLock.WaitAsync().ConfigureAwait(false);
                var stream = QpPackageHandler_Stream;
                if (stream == null)
                    throw new IOException("Connection is disconnected.");

                var ret = await getPackagePayloadFunc(sendPipe.Writer).ConfigureAwait(false);
                var packageType = ret.Item1;
                var packageBodyLength = ret.Item2;
                var packageTotalLength = packageBodyLength + PACKAGE_HEAD_LENGTH;

                if (packageTotalLength < PACKAGE_HEAD_LENGTH)
                    throw new IOException($"包大小[{packageTotalLength}]小于包头长度[{PACKAGE_HEAD_LENGTH}]");
                try
                {
                    ReadOnlySequence<byte> packageBuffer = default;
                    if (packageBodyLength > 0)
                    {
                        var readRet = await sendPipe.Reader.ReadAtLeastAsync(packageBodyLength).ConfigureAwait(false);
                        packageBuffer = readRet.Buffer;
                    }
                    await writePackageBuffer(stream,
                        packageType,
                        packageBuffer,
                        afterSendHandler).ConfigureAwait(false);
                    if (packageBodyLength > 0)
                    {
                        sendPipe.Reader.AdvanceTo(packageBuffer.End);
                    }
                    lastSendPackageTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    OnWriteError(ex);
                    throw;
                }
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
        public Task SendHeartbeatPackage()
        {
            return writePackageAsync(writer =>
            {
                //写入包类型
                writer.GetSpan(1)[0] = (byte)QpPackageType.Heartbeat;
                writer.Advance(1);
                return Task.FromResult(new Tuple<QpPackageType, int>(QpPackageType.Heartbeat, 1));
            }, null);
        }

        public Task SendNoticePackage(string noticePackageTypeName, string noticePackageContent)
        {
            return writePackageAsync(async writer =>
            {
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
                await writer.FlushAsync().ConfigureAwait(false);
                if (LogUtils.LogNotice)
                    LogUtils.Log("{0}: [Send-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                return new Tuple<QpPackageType, int>(QpPackageType.Notice, bodyLength);
            }, null);
        }

        /// <summary>
        /// 发送命令请求包
        /// </summary>
        public Task SendCommandRequestPackage(string commandId, string typeName, string content, Action afterSendHandler = null)
        {
            return writePackageAsync(async writer =>
            {
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
                await writer.FlushAsync().ConfigureAwait(false);

                if (LogUtils.LogCommand)
                    LogUtils.Log("{0}: [Send-CommandRequestPackage]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandId, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                return new Tuple<QpPackageType, int>(QpPackageType.CommandRequest, bodyLength);
            }, afterSendHandler);
        }

        /// <summary>
        /// 发送命令响应包
        /// </summary>
        public Task SendCommandResponsePackage(string commandId, byte code, string message, string typeName, string content)
        {
            return writePackageAsync(async writer =>
            {
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
                await writer.FlushAsync().ConfigureAwait(false);
                return new Tuple<QpPackageType, int>(QpPackageType.CommandResponse, bodyLength);
            }, null);
        }
    }
}
