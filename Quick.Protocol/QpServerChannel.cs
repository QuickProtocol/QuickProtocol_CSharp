using Quick.Protocol.Exceptions;

namespace Quick.Protocol
{
    public class QpServerChannel : QpChannel
    {
        private Stream stream;
        private CancellationTokenSource cts;
        private readonly CancellationToken serverCancellationToken;
        public new QpServerOptions Options { get; }
        private readonly string channelName;
        //通过认证后，才允许使用的命令执行管理器列表
        private readonly List<CommandExecuterManager> authedCommandExecuterManagerList = null;
        //通过认证后，才允许使用的通知处理器管理器列表
        private readonly List<NoticeHandlerManager> authedNoticeHandlerManagerList = null;

        public override string ChannelName => channelName;
        public Stream GetStream() => stream;

        /// <summary>
        /// 通过认证时
        /// </summary>
        internal event EventHandler Authenticated;
        /// <summary>
        /// 认证超时
        /// </summary>
        public event EventHandler AuthenticateTimeout;
        protected override bool ReadFromStreamReturnZeroMeansFault { get; }

        public QpServerChannel(Stream channelStream, string channelName, CancellationToken cancellationToken, QpServerOptions options, bool readFromStreamReturnZeroMeansFault = true) : base(options)
        {
            this.stream = channelStream;
            this.channelName = channelName;
            Options = options;
            this.authedCommandExecuterManagerList = options.CommandExecuterManagerList;
            this.authedNoticeHandlerManagerList = options.NoticeHandlerManagerList;
            serverCancellationToken = cancellationToken;
            ReadFromStreamReturnZeroMeansFault = readFromStreamReturnZeroMeansFault;

            cts = new CancellationTokenSource();

            //初始化连接相关指令处理器
            var connectAndAuthCommandExecuterManager = new CommandExecuterManager();
            connectAndAuthCommandExecuterManager.Register(new Commands.Connect.Request(), connect);
            connectAndAuthCommandExecuterManager.Register(new Commands.Authenticate.Request(), authenticate);
            connectAndAuthCommandExecuterManager.Register(new Commands.HandShake.Request(), handShake);
            connectAndAuthCommandExecuterManager.Register(new Commands.GetQpInstructions.Request(), getQpInstructions);

            ClearCommandExecuterManagers();
            RegisterCommandExecuterManagers([connectAndAuthCommandExecuterManager]);
            ClearNoticeHandlerManagers();

            InitQpPackageHandler_Stream(channelStream);
            var token = cts.Token;
            //开始读取其他数据包
            BeginReadPackage(token);
            //开始统计网络数据
            _ = BeginNetstat(token);
            //开始检查服务端的取消令牌
            _ = BeginCheckServerCancellationToken(token);

            //如果认证超时时间后没有通过认证，则断开连接
            if (options.AuthenticateTimeout > 0)
                _ = Task.Delay(options.AuthenticateTimeout, token).ContinueWith(t =>
                {
                    //如果已经取消或者已经连接
                    if (t.IsCanceled
                    || IsConnected)
                        return;
                    if (options.Logger is { LogConnection: true })
                        options.Logger.Log("{0} Authenticate timeout.", channelName);

                    Dispose();
                    AuthenticateTimeout?.Invoke(this, EventArgs.Empty);
                });
        }

        private async Task BeginCheckServerCancellationToken(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                if (serverCancellationToken.IsCancellationRequested)
                    Dispose();
            }
        }

        private Commands.Connect.Response connect(QpChannel handler, Commands.Connect.Request request)
        {
            if (request.InstructionIds != null)
            {
                foreach (var id in request.InstructionIds.Where(t => !string.IsNullOrEmpty(t)))
                {
                    if (!Options.InstructionSet.Any(t => t.Id == id))
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
            if (ComputeMD5Hash(AuthenticateQuestion + Options.Password) != request.Answer)
            {
                _ = Task.Delay(1000).ContinueWith(t =>
                {
                    Dispose();
                });
                throw new CommandException(1, "Authenticate failed.");
            }
            return new Commands.Authenticate.Response();
        }

        private Commands.HandShake.Response handShake(QpChannel handler, Commands.HandShake.Request request)
        {
            if (request.TransportTimeout < 3000)
                throw new ArgumentException($"'TransportTimeout' must greater than 3000");
            RegisterCommandExecuterManagers(authedCommandExecuterManagerList);
            RegisterNoticeHandlerManagers(authedNoticeHandlerManagerList);
            EnableCompress = request.EnableCompress;
            EnableEncrypt = request.EnableEncrypt;
            EncryptMethod  = request.EncryptMethod;
            EncryptMode = request.EncryptMode;
            EncryptPadding = request.EncryptPadding;
            TransportTimeout = request.TransportTimeout;
            OnAuthPassed();

            //改变传输超时时间
            ChangeTransportTimeout();

            //开始心跳
            if (HeartBeatInterval > 0)
                _ = BeginHeartBeat(cts.Token);
            IsConnected = true;
            Authenticated?.Invoke(this, EventArgs.Empty);
            return new Commands.HandShake.Response();
        }

        private Commands.GetQpInstructions.Response getQpInstructions(QpChannel handler, Commands.GetQpInstructions.Request request)
        {
            return new Commands.GetQpInstructions.Response()
            {
                Data = Options.InstructionSet
            };
        }

        public override void Disconnect()
        {
            base.Disconnect();
            try
            {
                cts?.Cancel();
                cts?.Dispose();
                cts = null;
                stream?.Dispose();
                stream = null;
            }
            catch { }
        }
        protected override void OnWriteError(Exception exception)
        {
            base.OnWriteError(exception);
            Dispose();
        }

        protected override void OnReadError(Exception exception)
        {
            if (Options.ProtocolErrorHandler != null)
            {
                if (exception is ProtocolException protocolException)
                {
                    if (Options.Logger is { LogConnection: true })
                        Options.Logger.Log("[ProtocolErrorHandler]{0}: Begin ProtocolErrorHandler invoke...", DateTime.Now);

                    Options.ProtocolErrorHandler.Invoke(stream, protocolException.ReadBuffer);
                    return;
                }
            }
            base.OnReadError(exception);
            Dispose();
        }
    }
}
