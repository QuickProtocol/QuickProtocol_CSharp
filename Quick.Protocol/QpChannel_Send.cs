using System.Buffers;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Text;
using Quick.Protocol.Streams;
using Quick.Utils;
using System.Buffers.Binary;
using System.Security.Cryptography;

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
            InitChannelStream(null);
            Disconnect();
        }

        //发送压缩相关变量
        private Pipe writeCompressPipe = null;
        //发送加密相关变量
        private Pipe writeEncryptPipe = null;

        private async Task writePackageBuffer(PipeReader currentReader, byte packageType,
            int packageBodyLength, bool ignoreCompressAndEncrypt = false)
        {
            var stream = channelStream;
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
                (EnableCompress || EnableEncrypt))
            {
                //如果压缩
                if (EnableCompress)
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
                        await writeCompressPipe.Writer.FlushAsync().ConfigureAwait(false);
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
                if (EnableEncrypt)
                {
                    //准备管道
                    if (writeEncryptPipe == null)
                        writeEncryptPipe = new Pipe();

                    packageBodyLength = 0;
                    try
                    {
                        //开始加密
                        using (var readMs = new ReadOnlySequenceByteStream(packageBodyBuffer))
                        using (var writeMs = new PipeWriterStream(writeEncryptPipe.Writer, true))
                        using (var encryptStream = new CryptoStream(writeMs, enc, CryptoStreamMode.Write))
                        {
                            await readMs.CopyToAsync(encryptStream);
                            await encryptStream.FlushFinalBlockAsync();
                            await encryptStream.FlushAsync();
                            packageBodyLength = Convert.ToInt32(writeMs.Length);
                            await writeEncryptPipe.Writer.FlushAsync().ConfigureAwait(false);
                        }
                        //加密完成，释放缓存
                        currentReader?.AdvanceTo(packageBodyBuffer.End);
                        var ret = await writeEncryptPipe.Reader.ReadAtLeastAsync(packageBodyLength);                        
                        packageBodyBuffer = ret.Buffer;

                        //包总长度
                        packageTotalLength = PACKAGE_HEAD_LENGTH + packageBodyLength;
                        currentReader = writeEncryptPipe.Reader;
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
                BinaryPrimitives.WriteInt32BigEndian(headMemory.Span, packageTotalLength);
                headMemory.Span[4] = packageType;
                writer.Advance(PACKAGE_HEAD_LENGTH);
                //包体
                if (packageBodyLength > 0)
                {
                    var bodyMemory = writer.GetMemory(packageBodyLength);
                    packageBodyBuffer.CopyTo(bodyMemory.Span);
                    writer.Advance(packageBodyLength);
                }

                await writer.FlushAsync().ConfigureAwait(false);

                //发送
                var reader = sendRawPipe.Reader;
                var rawRet = await reader.ReadAtLeastAsync(packageTotalLength);
                using (var sequenceByteStream = new ReadOnlySequenceByteStream(rawRet.Buffer))
                    await sequenceByteStream.CopyToAsync(stream)
                        .WaitAsync(TimeSpan.FromMilliseconds(TransportTimeout))
                        .ConfigureAwait(false);
                if (Options.EnableNetstat)
                {
                    BytesSent += packageTotalLength;
                    if (BytesSent > LONG_HALF_MAX_VALUE)
                        BytesSent = 0;
                }

                if (Options.Logger is { LogRaw: true })
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
        
        /// <summary>
        /// 发送不带包体的包
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns></returns>
        public Task SendPackage(byte packageType)
        {
            return SendPackage(packageType, null, false);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packageType"></param>
        /// <param name="packageBodyProvider"></param>
        /// <returns></returns>
        public Task SendPackage(byte packageType, PackageBodyProvider packageBodyProvider)
        {
            return SendPackage(packageType, packageBodyProvider, false);
        }

        private async Task SendPackage(byte packageType, PackageBodyProvider packageBodyProvider,
            bool ignoreCompressAndEncrypt)
        {
            var currentSendLock = sendLock;
            try
            {
                if (currentSendLock == null)
                    return;
                await currentSendLock.WaitAsync().ConfigureAwait(false);
                var packageBodyLength = 0;
                if (packageBodyProvider != null)
                    packageBodyLength = await packageBodyProvider(sendPipe.Writer).ConfigureAwait(false);
                await writePackageBuffer(sendPipe.Reader, packageType, packageBodyLength, ignoreCompressAndEncrypt);
            }
            catch (Exception ex)
            {
                OnWriteError(ex);
                throw new Exception("发送数据时出错", ex);
            }
            finally
            {
                currentSendLock?.Release();
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        public async Task SendHeartbeatPackage()
        {
            await SendPackage(PACKAGETYPE_HEARTBEAT).ConfigureAwait(false);
        }

        public async Task SendNoticePackage(string noticePackageTypeName, string noticePackageContent)
        {
            await SendPackage(PACKAGETYPE_NOTICE, async writer =>
            {
                var typeName = noticePackageTypeName;
                var content = noticePackageContent;
                var bodyLength = 0;
                //写入类名和长度
                {
                    var typeNameByteLength = encoding.GetByteCount(typeName);
                    writer.GetSpan(1)[0] = Convert.ToByte(typeNameByteLength);
                    writer.Advance(1);
                    bodyLength += 1;

                    encoding.GetBytes(typeName, writer.GetSpan(typeNameByteLength));
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
                        encoding.GetBytes(tmpCharMemory.Span, writer.GetSpan(byteCount));
                        writer.Advance(byteCount);
                        bodyLength += byteCount;
                    }
                }
                await writer.FlushAsync().ConfigureAwait(false);
                if (Options.Logger is { LogNotice: true })
                    Options.Logger.Log("{0}: [Send-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, Options
                        .Logger.LogContent
                        ? content
                        : QpLogger.NOT_SHOW_CONTENT_MESSAGE);
                return bodyLength;
            }).ConfigureAwait(false);
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
            await SendPackage(PACKAGETYPE_COMMAND_REQUEST, async writer =>
            {
                var bodyLength = 0;
                //写入指令编号
                {
                    var commandIdLength = commandId.Length / 2;
                    Convert.FromHexString(commandId, writer.GetMemory(commandIdLength).Span, out _, out var _);
                    writer.Advance(commandIdLength);
                    bodyLength += commandIdLength;
                }
                //写入类名和长度
                {
                    var typeNameByteLength = encoding.GetByteCount(typeName);
                    writer.GetSpan(1)[0] = Convert.ToByte(typeNameByteLength);
                    writer.Advance(1);
                    bodyLength += 1;

                    encoding.GetBytes(typeName, writer.GetSpan(typeNameByteLength));
                    writer.Advance(typeNameByteLength);
                    bodyLength += typeNameByteLength;
                }
                //写入内容
                {
                    var contentLength = encoding.GetByteCount(content);
                    encoding.GetBytes(content, writer.GetSpan(contentLength));
                    writer.Advance(contentLength);
                    bodyLength += contentLength;
                }
                await writer.FlushAsync().ConfigureAwait(false);
                if (Options.Logger is { LogCommand: true })
                    Options.Logger.Log("{0}: [Send-CommandRequestPackage]CommandId:{1},Type:{2},Content:{3}",
                        DateTime.Now, commandId, typeName, Options
                            .Logger.LogContent
                            ? content
                            : QpLogger.NOT_SHOW_CONTENT_MESSAGE);

                return bodyLength;
            }, ignoreCompressAndEncrypt).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送命令响应包
        /// </summary>
        public async Task SendCommandResponsePackage(string commandId, byte code, string message, string typeName,
            string content)
        {
            await SendPackage(PACKAGETYPE_COMMAND_RESPONSE, async writer =>
            {
                var bodyLength = 0;
                //写入指令编号
                {
                    var commandIdLength = commandId.Length / 2;
                    Convert.FromHexString(commandId, writer.GetMemory(commandIdLength).Span, out _, out var _);
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

                        encoding.GetBytes(typeName, writer.GetSpan(typeNameByteLength));
                        writer.Advance(typeNameByteLength);
                        bodyLength += typeNameByteLength;
                    }
                    //写入内容
                    {
                        var contentLength = encoding.GetByteCount(content);
                        encoding.GetBytes(content, writer.GetSpan(contentLength));
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
                        encoding.GetBytes(message, writer.GetSpan(messageLength));
                        writer.Advance(messageLength);
                        bodyLength += messageLength;
                    }
                    if (Options.Logger is { LogNotice: true })
                        Options.Logger.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Message:{3}",
                            DateTime.Now, commandId, code, message);
                }

                await writer.FlushAsync().ConfigureAwait(false);
                return bodyLength;
            }).ConfigureAwait(false);
        }
    }
}
