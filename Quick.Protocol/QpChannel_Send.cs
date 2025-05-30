﻿using Quick.Protocol.Utils;
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
using Quick.Protocol.Streams;

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
        
        private async Task writePackageBuffer(PipeReader currentReader, QpPackageType packageType, int packageBodyLength, bool ignoreCompressAndEncrypt = false)
        {
            var stream = QpPackageHandler_Stream;
            if (stream == null)
                throw new IOException("Not connected.");

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
                    using (var inStream = new ReadOnlySequenceByteStream(packageBodyBuffer))
                    using (var outStream = new PipeWriterStream(writeCompressPipe.Writer, true))
                    {
                        using (var gzStream = new GZipStream(outStream, CompressionMode.Compress, true))
                        {
                            await inStream.CopyToAsync(gzStream).ConfigureAwait(false);
                        }
                        packageTotalLength = Convert.ToInt32(outStream.Length);
                        _ = writeCompressPipe.Writer.FlushAsync();
                    }
                    //压缩完成，释放资源
                    currentReader?.AdvanceTo(packageBodyBuffer.End);

                    readRet = await writeCompressPipe.Reader.ReadAtLeastAsync(packageTotalLength).ConfigureAwait(false);
                    
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
                        //开始加密
                        var ret = enc.TransformFinalBlock(packageBodyBuffer.ToArray(), 0, (int)packageBodyBuffer.Length);
                        //加密完成，释放资源
                        currentReader?.AdvanceTo(packageBodyBuffer.End);

                        packageBodyBuffer = new ReadOnlySequence<byte>(ret);
                        packageTotalLength = ret.Length;

                        //包总长度
                        packageTotalLength += PACKAGE_TOTAL_LENGTH_LENGTH;
                        //准备包头
                        writePackageTotalLengthToBuffer(sendHeadBuffer, 0, packageTotalLength);
                        packageHeadMemory = new Memory<byte>(sendHeadBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH);
                        currentReader = null;
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
            var writeTask = stream.WriteAsync(packageHeadMemory).AsTask();
            await writeTask
                    .WaitAsync(TimeSpan.FromMilliseconds(options.InternalTransportTimeout))
                    .ConfigureAwait(false);
                    
            //如果有包内容，写入包内容
            if (packageBodyBuffer.Length > 0)
            {
                using (var sequenceByteStream = new ReadOnlySequenceByteStream(packageBodyBuffer))
                    writeTask = sequenceByteStream.CopyToAsync(stream);
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
                    packageType,
                    LogUtils.LogContent ?
                        BitConverter.ToString(packageHeadMemory.ToArray()) + "-" + BitConverter.ToString(packageBodyBuffer.ToArray())
                        : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
            currentReader?.AdvanceTo(packageBodyBuffer.End);
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
            await sendLock.WaitAsync().ConfigureAwait(false);
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
                var bodyLength = 1;
                _ = writer.FlushAsync();
                await writePackageBuffer(pipe.Reader,
                        QpPackageType.Heartbeat,
                        bodyLength).ConfigureAwait(false);
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
                        QpPackageType.Notice,
                        bodyLength).ConfigureAwait(false);
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
                        QpPackageType.CommandRequest,
                        bodyLength,
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
                        QpPackageType.CommandResponse,
                        bodyLength).ConfigureAwait(false);
            });
        }
    }
}
