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
using Quick.Protocol.Streams;

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

        private async Task FillRecvPipeAsync(Stream stream, PipeWriter writer, CancellationToken token)
        {
            var readBuffer = new byte[minimumBufferSize];
            while (!token.IsCancellationRequested)
            {
                var readTask = stream.ReadAsync(readBuffer, 0, readBuffer.Length, token)
                    .WaitAsync(TimeSpan.FromMilliseconds(options.InternalTransportTimeout), token)
                    .ConfigureAwait(false);
                int bytesRead = await readTask;
                if (bytesRead == 0)
                    continue;
                writer.Write(new ReadOnlySpan<byte>(readBuffer, 0, bytesRead));
                if (options.EnableNetstat)
                {
                    BytesReceived += bytesRead;
                    if (BytesReceived > LONG_HALF_MAX_VALUE)
                        BytesReceived = 0;
                }
                await writer.FlushAsync(token);
            }
        }

        private async Task ReadRecvPipeAsync(PipeReader recvReader, CancellationToken token)
        {
            //暂存包头缓存
            var packageHeadBuffer = new byte[PACKAGE_HEAD_LENGTH];
            //包总长度
            var packageTotalLength = 0;
            //解密相关变量
            Pipe decryptPipe = null;
            //解压相关变量
            Pipe decompressPipe = null;

            while (!token.IsCancellationRequested)
            {
                var currentReader = recvReader;
                var readTask = currentReader.ReadAtLeastAsync(PACKAGE_TOTAL_LENGTH_LENGTH, token);
                var ret = await readTask.AsTask()
                    .WaitAsync(TimeSpan.FromMilliseconds(options.InternalTransportTimeout), token)
                    .ConfigureAwait(false);
                if (ret.IsCanceled)
                    return;
                if (ret.Buffer.Length < PACKAGE_TOTAL_LENGTH_LENGTH)
                    throw new ProtocolException(ret.Buffer, $"包头读取错误！包头长度：{PACKAGE_TOTAL_LENGTH_LENGTH}，读取数据长度：{ret.Buffer.Length}");

                //解析包总长度
                packageTotalLength = parsePackageTotalLength(ret.Buffer, packageHeadBuffer);
                currentReader.AdvanceTo(ret.Buffer.Start);

                //读取完整包
                ret = await currentReader.ReadAtLeastAsync(packageTotalLength, token).ConfigureAwait(false);
                if (ret.IsCanceled)
                    return;
                if (ret.Buffer.Length < packageTotalLength)
                    throw new ProtocolException(ret.Buffer, $"包读取错误！包总长度：{packageTotalLength}，读取数据长度：{ret.Buffer.Length}");
                var packageBuffer = ret.Buffer.Slice(0, packageTotalLength);
                if (LogUtils.LogRaw)
                {
                    var sb = new StringBuilder();
                    sb.Append($"{DateTime.Now}: [Recv-Raw]Length: {packageBuffer.Length}");
                    if (LogUtils.LogContent)
                        sb.Append(", Content: " + Convert.ToHexString(packageBuffer.ToArray()));
                    else
                        sb.Append(LogUtils.NOT_SHOW_CONTENT_MESSAGE);
                    LogUtils.Log(sb.ToString());
                }

                //如果不是心跳包，则启用了压缩或者加密
                if (packageTotalLength > PACKAGE_HEAD_LENGTH && (options.InternalCompress || options.InternalEncrypt))
                {
                    //如果设置了加密
                    if (options.InternalEncrypt)
                    {
                        //准备管道
                        if (decryptPipe == null)
                            decryptPipe = new Pipe();

                        //写入包头
                        decryptPipe.Writer.GetMemory(PACKAGE_TOTAL_LENGTH_LENGTH);
                        decryptPipe.Writer.Advance(PACKAGE_TOTAL_LENGTH_LENGTH);
                        packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH;

                        //开始解密
                        var encryptedBuffer = packageBuffer.Slice(PACKAGE_TOTAL_LENGTH_LENGTH).ToArray();
                        var decryptBuffer = dec.TransformFinalBlock(encryptedBuffer, 0, encryptedBuffer.Length);
                        packageTotalLength += decryptBuffer.Length;

                        decryptBuffer.CopyTo(decryptPipe.Writer.GetMemory(decryptBuffer.Length));
                        decryptPipe.Writer.Advance(decryptBuffer.Length);

                        _ = decryptPipe.Writer.FlushAsync();
                        ret = await decryptPipe.Reader.ReadAtLeastAsync(packageTotalLength, token).ConfigureAwait(false);

                        //解密完成，释放缓存
                        currentReader?.AdvanceTo(packageBuffer.End);

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
                        using (var readMs = new ReadOnlySequenceByteStream(compressedBuffer))
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
                        _ = decompressPipe.Writer.FlushAsync();
                        ret = await decompressPipe.Reader.ReadAtLeastAsync(packageTotalLength, token).ConfigureAwait(false);
                        //解压完成，释放缓存
                        currentReader?.AdvanceTo(packageBuffer.End);
                        packageBuffer = ret.Buffer;
                        currentReader = decompressPipe.Reader;
                    }
                }

                //包类型
                packageBuffer.Slice(0, PACKAGE_HEAD_LENGTH).CopyTo(packageHeadBuffer);
                var packageType = (QpPackageType)packageHeadBuffer[PACKAGE_TOTAL_LENGTH_LENGTH];

                HandlePackage(packageType, packageBuffer);
                currentReader?.AdvanceTo(packageBuffer.End);
            }
        }

        protected void HandlePackage(QpPackageType packageType, ReadOnlySequence<byte> packageBuffer)
        {
            //不带包长度和包类型的包体
            var bodyBuffer = packageBuffer.Slice(PACKAGE_HEAD_LENGTH);
            if (LogUtils.LogPackage)
            {
                var sb = new StringBuilder();
                sb.Append($"{DateTime.Now}: [Recv-Package]Type: {packageType}");
                if (bodyBuffer.Length > 0)
                {
                    if (LogUtils.LogContent)
                        sb.Append(", Content: "+Convert.ToHexString(bodyBuffer.ToArray()));
                    else
                        sb.Append(LogUtils.NOT_SHOW_CONTENT_MESSAGE);
                }
                LogUtils.Log(sb.ToString());
            }
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
            var pipe = new Pipe();
            FillRecvPipeAsync(QpPackageHandler_Stream, pipe.Writer, token).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    OnReadError(task.Exception);
                pipe.Writer.CompleteAsync(task.Exception);
            });
            ReadRecvPipeAsync(pipe.Reader, token).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    OnReadError(task.Exception);
                pipe.Reader.CompleteAsync(task.Exception);
            });
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
