using Quick.Protocol.Exceptions;
using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol
{
    public class QpServerChannel : QpChannel
    {
        private readonly Stream stream;
        private readonly CancellationTokenSource cts;
        private readonly QpServerOptions options;
        private readonly string channelName;
        //通过认证后，才允许使用的命令执行管理器列表
        private readonly List<CommandExecuterManager> authedCommandExecuterManagerList = null;
        //通过认证后，才允许使用的通知处理器管理器列表
        private readonly List<NoticeHandlerManager> authedNoticeHandlerManagerList = null;

        public override string ChannelName => channelName;

        /// <summary>
        /// 通过认证时
        /// </summary>
        internal event EventHandler Auchenticated;
        /// <summary>
        /// 认证超时
        /// </summary>
        public event EventHandler AuchenticateTimeout;

        public QpServerChannel(Stream stream, string channelName, CancellationToken cancellationToken, QpServerOptions options) : base(options)
        {
            this.stream = stream;
            this.channelName = channelName;
            this.options = options;
            this.authedCommandExecuterManagerList = options.CommandExecuterManagerList;
            this.authedNoticeHandlerManagerList = options.NoticeHandlerManagerList;

            cts = new CancellationTokenSource();
            cancellationToken.Register(() => Stop());

            //初始化连接相关指令处理器
            var connectAndAuthCommandExecuterManager = new CommandExecuterManager();
            connectAndAuthCommandExecuterManager.Register(new Commands.Connect.Request(), connect);
            connectAndAuthCommandExecuterManager.Register(new Commands.Authenticate.Request(), authenticate);
            connectAndAuthCommandExecuterManager.Register(new Commands.HandShake.Request(), handShake);
            connectAndAuthCommandExecuterManager.Register(new Commands.GetQpInstructions.Request(), getQpInstructions);
            options.CommandExecuterManagerList = new List<CommandExecuterManager>() { connectAndAuthCommandExecuterManager };
            options.NoticeHandlerManagerList = null;

            InitQpPackageHandler_Stream(stream);
            var token = cts.Token;
            //开始读取其他数据包
            BeginReadPackage(token);
            //开始统计网络数据
            BeginNetstat(token);

            //如果认证超时时间后没有通过认证，则断开连接
            if (options.AuthenticateTimeout > 0)
                _ = Task.Delay(options.AuthenticateTimeout, token).ContinueWith(t =>
                {
                    //如果已经取消或者已经连接
                    if (t.IsCanceled
                    || IsConnected)
                        return;
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]{0} Authenticate timeout.", channelName);

                    if (stream != null)
                    {
                        try
                        {
                            stream.Close();
                            stream.Dispose();
                            stream = null;
                        }
                        catch { }
                    }
                    AuchenticateTimeout?.Invoke(this, EventArgs.Empty);
                });
        }

        private Commands.Connect.Response connect(QpChannel handler, Commands.Connect.Request request)
        {
            if (request.InstructionIds != null)
            {
                foreach (var id in request.InstructionIds.Where(t => !string.IsNullOrEmpty(t)))
                {
                    if (!options.InstructionSet.Any(t => t.Id == id))
                        throw new CommandException(255, $"Unknown instruction: {id}");
                }
            }
            AuthenticateQuestion = Guid.NewGuid().ToString("N");
            return new Commands.Connect.Response()
            {
                Question = AuthenticateQuestion
            };
        }

        private Commands.Authenticate.Response authenticate(QpChannel handler, Commands.Authenticate.Request request)
        {
            if (Utils.CryptographyUtils.ComputeMD5Hash(AuthenticateQuestion + options.Password) != request.Answer)
            {
                _ = Task.Delay(1000).ContinueWith(t =>
                {
                    Stop();
                });
                throw new CommandException(1, "Authenticate failed.");
            }
            IsConnected = true;
            Auchenticated?.Invoke(this, EventArgs.Empty);
            return new Commands.Authenticate.Response();
        }

        private Commands.HandShake.Response handShake(QpChannel handler, Commands.HandShake.Request request)
        {
            options.CommandExecuterManagerList.AddRange(authedCommandExecuterManagerList);
            options.NoticeHandlerManagerList = authedNoticeHandlerManagerList;
            options.InternalCompress = request.EnableCompress;
            options.InternalEncrypt = request.EnableEncrypt;
            options.InternalTransportTimeout = request.TransportTimeout;

            //改变传输超时时间
            ChangeTransportTimeout();

            //开始心跳
            if (options.HeartBeatInterval > 0)
                BeginHeartBeat(cts.Token);
            return new Commands.HandShake.Response();
        }

        private Commands.GetQpInstructions.Response getQpInstructions(QpChannel handler, Commands.GetQpInstructions.Request request)
        {
            return new Commands.GetQpInstructions.Response()
            {
                Data = options.InstructionSet
            };
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            try
            {
                cts?.Cancel();
                stream?.Close();
                stream?.Dispose();
            }
            catch { }
        }
        protected override void OnWriteError(Exception exception)
        {
            Stop();
            base.OnWriteError(exception);
        }

        protected override void OnReadError(Exception exception)
        {
            if (options.ProtocolErrorHandler != null)
            {
                if (exception is ProtocolException)
                {
                    var protocolException = (ProtocolException)exception;
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[ProtocolErrorHandler]{0}: Begin ProtocolErrorHandler invoke...", DateTime.Now);

                    options.ProtocolErrorHandler.Invoke(stream, protocolException.ReadBuffer);
                    return;
                }
            }
            Stop();
            base.OnReadError(exception);
        }
    }
}
