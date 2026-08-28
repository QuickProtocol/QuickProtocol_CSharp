namespace Quick.Protocol
{
    public abstract class QpClient : QpChannel
    {
        private CancellationTokenSource cts = null;

        public new QpClientOptions Options { get; private set; }

        public QpClient(QpClientOptions options)
            : base(options)
        {
            options.Check();
            Options = options;
        }

        public override string ChannelName => Options.ToUri().ToString();
        protected abstract Task<Stream> InnerConnectAsync();

        /// <summary>
        /// 连接
        /// </summary>
        public async Task ConnectAsync()
        {
            if(IsDisposed)
                throw new ObjectDisposedException("QpClient has disposed.");

            //清理
            Disconnect();
            Init();

            RegisterCommandExecuterManagers(Options.CommandExecuterManagerList);
            RegisterNoticeHandlerManagers(Options.NoticeHandlerManagerList);

            cts = new CancellationTokenSource();
            var token = cts.Token;

            var stream = await InnerConnectAsync().ConfigureAwait(false);
            //初始化网络
            InitChannelStream(stream);

            //开始读取其他数据包
            BeginReadPackage(token);
            //开始统计网络数据
            _ = BeginNetstat(token);

            var repConnect = await SendCommand(new Commands.Connect.Request()
            {
                InstructionIds = Options.InstructionSet.Select(t => t.Id).ToArray()
            }).ConfigureAwait(false);
            AuthenticateQuestion = repConnect.Question;

            var repAuth = await SendCommand(new Commands.Authenticate.Request()
            {
                Answer = ComputeMD5Hash(AuthenticateQuestion + Options.Password)
            }).ConfigureAwait(false);

            EnableCompress = Options.EnableCompress;
            EnableEncrypt = Options.EnableEncrypt;
            EncryptAlgorithm  = Options.EncryptAlgorithm;
            EncryptMode = Options.EncryptMode;
            EncryptPadding = Options.EncryptPadding;
            OnAuthPassed();

            IsConnected = true;

            var repHandShake = await SendCommand(new Commands.HandShake.Request()
            {
                EnableCompress = Options.EnableCompress,
                EnableEncrypt = Options.EnableEncrypt,
                EncryptAlgorithm = Options.EncryptAlgorithm,
                EncryptMode = Options.EncryptMode,
                EncryptPadding = Options.EncryptPadding,
                TransportTimeout = Options.TransportTimeout
            }, 5000, true).ConfigureAwait(false);

            //开始心跳
            if (HeartBeatInterval > 0)
            {
                //定时发送心跳包
                _ = BeginHeartBeat(token);
                await Task.Delay(1000, token);
                await SendHeartbeatPackage();
            }
        }

        protected override void OnWriteError(Exception exception)
        {
            base.OnWriteError(exception);
            Disconnect();
        }

        protected override void OnReadError(Exception exception)
        {
            base.OnReadError(exception);
            Disconnect();
        }

        public override void Disconnect()
        {
            base.Disconnect();

            try { cts?.Cancel(); } catch { }
            try { cts?.Dispose(); } catch { }
            cts = null;

            TransportTimeout = Options.TransportTimeout;
            EnableCompress = false;
            EnableEncrypt = false;
        }
    }
}
