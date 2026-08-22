using System.Buffers;
using System.Text;
using Quick.Protocol.Exceptions;
using Quick.Utils;

namespace Quick.Protocol
{
    public abstract partial class QpChannel
    {
        private async ValueTask handleHeartbeatPackage(QpChannel qpChannel, byte packageType, ReadOnlySequence<byte> bodyBuffer)
        {
            if (Options.Logger is { LogHeartbeat: true })
                Options.Logger.Log("{0}: [Recv-HeartbeatPackage]", DateTime.Now);
            HeartbeatPackageReceived?.Invoke(this, EventArgs.Empty);
        }

        private async ValueTask handleNoticePackage(QpChannel qpChannel, byte packageType, ReadOnlySequence<byte> bodyBuffer)
        {
            var typeNameLength = bodyBuffer.First.Span[0];
            bodyBuffer = bodyBuffer.Slice(1);

            var typeName = encoding.GetString(bodyBuffer.Slice(0, typeNameLength));
            bodyBuffer = bodyBuffer.Slice(typeNameLength);

            var content = encoding.GetString(bodyBuffer);

            if (Options.Logger is { LogNotice: true })
                Options.Logger.Log("{0}: [Recv-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, Options
                    .Logger.LogContent ? content : QpLogger.NOT_SHOW_CONTENT_MESSAGE);
            //异步等待执行通知处理器
            await OnRawNoticePackageReceived(typeName, content);
        }

        private async ValueTask handleCommandRequestPackage(QpChannel qpChannel, byte packageType, ReadOnlySequence<byte> bodyBuffer)
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

            if (Options.Logger != null && Options.Logger.LogCommand)
                Options.Logger.Log("{0}: [Recv-CommandRequestPackage]Type:{1},Content:{2}", DateTime.Now, typeName, Options
                    .Logger.LogContent ? content : QpLogger.NOT_SHOW_CONTENT_MESSAGE);
            //异步执行命令请求事件处理器
            _ = OnCommandRequestReceived(commandId, typeName, content);
        }

        private async ValueTask handleCommandResponsePackage(QpChannel qpChannel, byte packageType, ReadOnlySequence<byte> bodyBuffer)
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

            if (Options.Logger is { LogCommand: true })
                Options.Logger.Log("{0}: [Recv-CommandResponsePackage]Code:{1}，Message：{2}，Type:{3},Content:{4}", DateTime.Now, code, message, typeName, Options
                    .Logger.LogContent ? content : QpLogger.NOT_SHOW_CONTENT_MESSAGE);

            OnCommandResponseReceived(commandId, code, message, typeName, content);
        }


        /// <summary>
        /// 接收到原始通知数据包时
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        protected async Task OnRawNoticePackageReceived(string typeName, string content)
        {
            //触发RawNoticePackageReceived事件
            RawNoticePackageReceived?.Invoke(this, new RawNoticePackageReceivedEventArgs()
            {
                TypeName = typeName,
                Content = content
            });
            //如果在字典中未找到此类型名称，则直接返回
            if (!noticeTypeDict.TryGetValue(typeName, out var noticeType))
                return;
            var noticeSerializer = getTypeSerializer(noticeType);
            var contentModel = noticeSerializer.Deserialize(content);

            //处理通知
            var hasNoticeHandler = false;
            foreach (var noticeHandlerManager in noticeHandlerManagerList)
            {
                if (noticeHandlerManager.CanHandleNoticed(typeName))
                {
                    hasNoticeHandler = true;
                    await noticeHandlerManager.HandleNotice(this, typeName, contentModel);
                    break;
                }
            }

            //如果配置了触发NoticePackageReceived事件
            if (Options.RaiseNoticePackageReceivedEvent)
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
        private async Task OnCommandRequestReceived(string commandId, string typeName, string content)
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
                if (!commandRequestTypeDict.TryGetValue(typeName, out var cmdRequestType))
                    throw new CommandException(255, $"Unknown RequestType[{typeName}].");
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
                foreach (var commandExecuterManager in commandExecuterManagerList)
                {
                    if (commandExecuterManager.CanExecuteCommand(typeName))
                    {
                        hasCommandExecuter = true;
                        var responseModel = await commandExecuterManager.ExecuteCommand(this, typeName, contentModel);
                        var responseSerializer = getTypeSerializer(cmdResponseType);
                        _ = SendCommandResponsePackage(commandId, 0, null,
                            cmdResponseType.FullName,
                            responseSerializer.Serialize(responseModel));
                        break;
                    }
                }
                if (!hasCommandExecuter)
                    throw new CommandException(255, $"No CommandExecuter for RequestType[{typeName}]");
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
    }
}
