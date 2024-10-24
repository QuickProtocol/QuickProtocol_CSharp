using Quick.Protocol.Exceptions;
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
        /// <summary>
        /// 当读取出错时
        /// </summary>
        protected virtual void OnReadError(Exception exception)
        {
            LastException = exception;
            LogUtils.Log("[ReadError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            InitQpPackageHandler_Stream(null);
            Disconnect();
        }

        /// <summary>
        /// 接收到原始通知数据包时
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        protected void OnRawNoticePackageReceived(string typeName, string content)
        {
            //触发RawNoticePackageReceived事件
            RawNoticePackageReceived?.Invoke(this, new RawNoticePackageReceivedEventArgs()
            {
                TypeName = typeName,
                Content = content
            });
            //如果在字典中未找到此类型名称，则直接返回
            if (!noticeTypeDict.ContainsKey(typeName))
                return;
            var noticeType = noticeTypeDict[typeName];
            var noticeSerializer = getTypeSerializer(noticeType);
            var contentModel = noticeSerializer.Deserialize(content);

            //处理通知
            var hasNoticeHandler = false;
            if (options.NoticeHandlerManagerList != null)
                foreach (var noticeHandlerManager in options.NoticeHandlerManagerList)
                {
                    if (noticeHandlerManager.CanHandleNoticed(typeName))
                    {
                        hasNoticeHandler = true;
                        noticeHandlerManager.HandleNotice(this, typeName, contentModel);
                        break;
                    }
                }

            //如果配置了触发NoticePackageReceived事件
            if (options.RaiseNoticePackageReceivedEvent)
            {
                NoticePackageReceived?.Invoke(this, new NoticePackageReceivedEventArgs()
                {
                    TypeName = typeName,
                    ContentModel = contentModel,
                    Handled = hasNoticeHandler
                });
            }
        }

        /// <summary>
        /// 接收到命令请求数据包时
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        private void OnCommandRequestReceived(string commandId, string typeName, string content)
        {
            var eventArgs = new RawCommandRequestPackageReceivedEventArgs()
            {
                CommandId = commandId,
                TypeName = typeName,
                Content = content
            };
            RawCommandRequestPackageReceived?.Invoke(this, eventArgs);
            //如果已经处理，则直接返回
            if (eventArgs.Handled)
                return;

            try
            {
                //如果在字典中未找到此类型名称，则直接返回
                if (!commandRequestTypeDict.ContainsKey(typeName))
                    throw new CommandException(255, $"Unknown RequestType: {typeName}.");

                var cmdRequestType = commandRequestTypeDict[typeName];
                var cmdResponseType = commandRequestTypeResponseTypeDict[cmdRequestType];
                var requestSerilizer = getTypeSerializer(cmdRequestType);
                var contentModel = requestSerilizer.Deserialize(content);
                CommandRequestPackageReceived?.Invoke(this, new CommandRequestPackageReceivedEventArgs()
                {
                    CommandId = commandId,
                    TypeName = typeName,
                    ContentModel = contentModel
                });

                var hasCommandExecuter = false;
                if (options.CommandExecuterManagerList != null)
                    foreach (var commandExecuterManager in options.CommandExecuterManagerList)
                    {
                        if (commandExecuterManager.CanExecuteCommand(typeName))
                        {
                            hasCommandExecuter = true;
                            var responseModel = commandExecuterManager.ExecuteCommand(this, typeName, contentModel);
                            var responseSerializer = getTypeSerializer(cmdResponseType);
                            _ = SendCommandResponsePackage(commandId, 0, null,
                                cmdResponseType.FullName,
                                responseSerializer.Serialize(responseModel));
                            break;
                        }
                    }
                if (!hasCommandExecuter)
                    throw new CommandException(255, $"No CommandExecuter for RequestType:{typeName}");
            }
            catch (CommandException ex)
            {
                string errorMessage = ExceptionUtils.GetExceptionMessage(ex);
                _ = SendCommandResponsePackage(commandId, ex.Code, errorMessage, null, null);
            }
            catch (Exception ex)
            {
                string errorMessage = ExceptionUtils.GetExceptionMessage(ex);
                _ = SendCommandResponsePackage(commandId, 255, errorMessage, null, null);
            }
        }

        /// <summary>
        /// 接收到命令响应数据包时
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        private void OnCommandResponseReceived(string commandId, byte code, string message, string typeName, string content)
        {
            CommandResponsePackageReceived?.Invoke(this, new CommandResponsePackageReceivedEventArgs()
            {
                CommandId = commandId,
                Code = code,
                Message = message,
                TypeName = typeName,
                Content = content
            });
            //设置指令响应
            CommandContext commandContext;
            if (!commandDict.TryRemove(commandId, out commandContext))
                return;
            if (code == 0)
                commandContext.SetResponse(typeName, content);
            else
                commandContext.SetResponse(new CommandException(code, message));
        }

        private async Task ReadRecvPipeAsync(PipeReader recvReader, CancellationToken token)
        {
            //暂存包头缓存
            var packageHeadBuffer = new byte[PACKAGE_HEAD_LENGTH];
            //包总长度
            var packageTotalLength = 0;
            //解密相关变量
            Pipe decryptPipe = null;
            byte[] decryptBuffer1 = null;
            byte[] decryptBuffer2 = null;
            //解压相关变量
            Pipe decompressPipe = null;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var currentReader = recvReader;
                    var ret = await currentReader.ReadAtLeastAsync(PACKAGE_HEAD_LENGTH, token).ConfigureAwait(false);
                    if (ret.IsCanceled)
                        return;
                    //解析包总长度
                    packageTotalLength = parsePackageTotalLength(ret.Buffer, packageHeadBuffer);
                    currentReader.AdvanceTo(ret.Buffer.Start);

                    //读取完整包
                    ret = await recvReader.ReadAtLeastAsync(packageTotalLength, token).ConfigureAwait(false);
                    if (ret.IsCanceled)
                        return;
                    if (ret.Buffer.Length < packageTotalLength)
                        throw new ProtocolException(ret.Buffer, $"包读取错误！包总长度：{packageTotalLength}，读取数据长度：{ret.Buffer.Length}");
                    var packageBuffer = ret.Buffer.Slice(0, packageTotalLength);

                    //如果设置了压缩或者加密
                    if (options.InternalCompress || options.InternalEncrypt)
                    {
                        //如果设置了加密
                        if (options.InternalEncrypt)
                        {
                            //准备管道
                            if (decryptPipe == null)
                            {
                                decryptPipe = new Pipe();
                                decryptBuffer1 = new byte[1024];
                                decryptBuffer2 = new byte[1024];
                            }

                            //写入包头
                            decryptPipe.Writer.GetMemory(PACKAGE_TOTAL_LENGTH_LENGTH);
                            decryptPipe.Writer.Advance(PACKAGE_TOTAL_LENGTH_LENGTH);
                            packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH;

                            //开始解密
                            var encryptedBuffer = packageBuffer.Slice(PACKAGE_TOTAL_LENGTH_LENGTH);
                            while (encryptedBuffer.Length > 0)
                            {
                                var inLength = Math.Min(decryptBuffer1.Length, (int)encryptedBuffer.Length);
                                encryptedBuffer.Slice(0, inLength).CopyTo(decryptBuffer1);
                                var outLength = dec.TransformBlock(decryptBuffer1, 0, inLength, decryptBuffer2, 0);
                                decryptBuffer2.CopyTo(decryptPipe.Writer.GetMemory(outLength));
                                decryptPipe.Writer.Advance(outLength);
                                encryptedBuffer = encryptedBuffer.Slice(inLength);
                                packageTotalLength += outLength;
                            }
                            {
                                var finalData = dec.TransformFinalBlock(decryptBuffer1, 0, 0);
                                finalData.CopyTo(decryptPipe.Writer.GetMemory(finalData.Length));
                                decryptPipe.Writer.Advance(finalData.Length);
                                packageTotalLength += finalData.Length;
                            }
                            await decryptPipe.Writer.FlushAsync().ConfigureAwait(false);
                            //解密完成，释放缓存
                            currentReader.AdvanceTo(packageBuffer.End);

                            ret = await decryptPipe.Reader.ReadAtLeastAsync(packageTotalLength, token).ConfigureAwait(false);
                            packageBuffer = ret.Buffer;
                            currentReader = decryptPipe.Reader;
                        }

                        //如果设置了压缩
                        if (options.InternalCompress)
                        {
                            //准备管道
                            if (decompressPipe == null)
                                decompressPipe = new Pipe();

                            //写入包头
                            decompressPipe.Writer.GetMemory(PACKAGE_TOTAL_LENGTH_LENGTH);
                            decompressPipe.Writer.Advance(PACKAGE_TOTAL_LENGTH_LENGTH);
                            packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH;

                            //开始解压
                            var compressedBuffer = packageBuffer.Slice(PACKAGE_TOTAL_LENGTH_LENGTH);
                            using (var readMs = compressedBuffer.AsStream())
                            using (var gzStream = new GZipStream(readMs, CompressionMode.Decompress, true))
                            {
                                while (true)
                                {
                                    var count = await gzStream.ReadAsync(decompressPipe.Writer.GetMemory(minimumBufferSize), token).ConfigureAwait(false);
                                    if (count <= 0)
                                        break;
                                    decompressPipe.Writer.Advance(count);
                                    packageTotalLength += count;
                                }
                            }
                            await decompressPipe.Writer.FlushAsync().ConfigureAwait(false);
                            //解压完成，释放缓存
                            currentReader.AdvanceTo(packageBuffer.End);

                            ret = await decompressPipe.Reader.ReadAtLeastAsync(packageTotalLength, token).ConfigureAwait(false);
                            packageBuffer = ret.Buffer;
                            currentReader = decompressPipe.Reader;
                        }
                    }

                    //包类型
                    packageBuffer.Slice(0, PACKAGE_HEAD_LENGTH).CopyTo(packageHeadBuffer);
                    var packageType = (QpPackageType)packageHeadBuffer[PACKAGE_TOTAL_LENGTH_LENGTH];

                    if (LogUtils.LogPackage)
                        LogUtils.Log(
                        "{0}: [Recv-Package]Length:{1}，Type:{2}，Content:{3}",
                        DateTime.Now,
                        packageTotalLength,
                        packageType,
                        LogUtils.LogContent ?
                            BitConverter.ToString(packageBuffer.ToArray())
                            : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
                    HandlePackage(packageType, packageBuffer);
                    currentReader.AdvanceTo(packageBuffer.End);
                }
            }
            catch (Exception ex)
            {
                OnReadError(ex);
            }
        }

        protected void HandlePackage(QpPackageType packageType, ReadOnlySequence<byte> packageBuffer)
        {
            var bodyBuffer = packageBuffer.Slice(PACKAGE_HEAD_LENGTH);
            switch (packageType)
            {
                case QpPackageType.Heartbeat:
                    {
                        if (LogUtils.LogHeartbeat)
                            LogUtils.Log("{0}: [Recv-HeartbeatPackage]", DateTime.Now);
                        HeartbeatPackageReceived?.Invoke(this, QpEventArgs.Empty);
                        break;
                    }
                case QpPackageType.Notice:
                    {
                        var typeNameLength = bodyBuffer.First.Span[0];
                        bodyBuffer = bodyBuffer.Slice(1);

                        var typeName = encoding.GetString(bodyBuffer.Slice(0, typeNameLength));
                        bodyBuffer = bodyBuffer.Slice(typeNameLength);

                        var content = encoding.GetString(bodyBuffer);

                        if (LogUtils.LogNotice)
                            LogUtils.Log("{0}: [Recv-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                        OnRawNoticePackageReceived(typeName, content);
                        break;
                    }
                case QpPackageType.CommandRequest:
                    {
                        var commandId = Convert.ToHexString(bodyBuffer.Slice(0, COMMAND_ID_LENGTH).ToArray()).ToLower();
                        bodyBuffer = bodyBuffer.Slice(COMMAND_ID_LENGTH);

                        var typeNameLength = bodyBuffer.First.Span[0];
                        bodyBuffer = bodyBuffer.Slice(1);
                        if (bodyBuffer.Length < typeNameLength)
                        {
                            throw new IOException($"bodyBuffer.Length:{bodyBuffer.Length} < TypeNameLength: {typeNameLength}，Content:{encoding.GetString(bodyBuffer)}");
                        }
                        var typeName = encoding.GetString(bodyBuffer.Slice(0, typeNameLength));
                        bodyBuffer = bodyBuffer.Slice(typeNameLength);

                        var content = encoding.GetString(bodyBuffer);

                        if (LogUtils.LogCommand)
                            LogUtils.Log("{0}: [Recv-CommandRequestPackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
                        //异步执行命令请求事件处理器
                        Task.Run(() => OnCommandRequestReceived(commandId, typeName, content));
                        break;
                    }
                case QpPackageType.CommandResponse:
                    {
                        var commandId = Convert.ToHexString(bodyBuffer.Slice(0, COMMAND_ID_LENGTH).ToArray()).ToLower();
                        bodyBuffer = bodyBuffer.Slice(COMMAND_ID_LENGTH);

                        var code = bodyBuffer.First.Span[0];
                        bodyBuffer = bodyBuffer.Slice(1);

                        string typeName = null;
                        string content = null;
                        string message = null;

                        //如果成功
                        if (code == 0)
                        {
                            var typeNameLength = bodyBuffer.First.Span[0];
                            bodyBuffer = bodyBuffer.Slice(1);

                            if (bodyBuffer.Length < typeNameLength)
                            {
                                throw new IOException($"bodyBuffer.Length:{bodyBuffer.Length} < TypeNameLength: {typeNameLength}，Content:{encoding.GetString(bodyBuffer)}");
                            }
                            typeName = encoding.GetString(bodyBuffer.Slice(0, typeNameLength));
                            bodyBuffer = bodyBuffer.Slice(typeNameLength);

                            content = encoding.GetString(bodyBuffer);
                        }
                        else
                        {
                            message = encoding.GetString(bodyBuffer);
                        }

                        if (LogUtils.LogCommand)
                            LogUtils.Log("{0}: [Recv-CommandResponsePackage]Code:{1}，Message：{2}，Type:{3},Content:{4}", DateTime.Now, code, message, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                        OnCommandResponseReceived(commandId, code, message, typeName, content);
                        break;
                    }
            }
        }


        protected void BeginReadPackage(CancellationToken token)
        {
            _ = Task.Run(async () =>
            {
                var pipe = new Pipe();
                var fillTask = FillRecvPipeAsync(QpPackageHandler_Stream, pipe.Writer, token);
                var readTask = ReadRecvPipeAsync(pipe.Reader, token);
                try
                {
                    await Task.WhenAll(fillTask, readTask).ConfigureAwait(false);
                }
                catch
                {
                    await pipe.Writer.CompleteAsync().ConfigureAwait(false);
                    await pipe.Reader.CompleteAsync().ConfigureAwait(false);
                }
            });
        }

        private async Task FillRecvPipeAsync(Stream stream, PipeWriter writer, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                    int bytesRead = await stream.ReadAsync(memory, token);
                    if (bytesRead == 0)
                        break;
                    writer.Advance(bytesRead);
                    await writer.FlushAsync(token);
                }
            }
            catch (Exception ex)
            {
                OnReadError(ex);
            }
        }

        //解析包总长度
        private int parsePackageTotalLength(ReadOnlySequence<byte> sequence, byte[] buffer)
        {
            sequence.Slice(0, PACKAGE_TOTAL_LENGTH_LENGTH).CopyTo(buffer);
            var packageTotalLength = ByteUtils.B2I_BE(buffer, 0);
            if (packageTotalLength < PACKAGE_HEAD_LENGTH)
                throw new ProtocolException(new ReadOnlySequence<byte>(buffer), $"包长度[{packageTotalLength}]必须大于等于{PACKAGE_HEAD_LENGTH}！");
            if (packageTotalLength > options.MaxPackageSize)
                throw new ProtocolException(new ReadOnlySequence<byte>(buffer), $"包长度[{packageTotalLength}]大于最大包大小[{options.MaxPackageSize}]");
            return packageTotalLength;
        }

    }
}
