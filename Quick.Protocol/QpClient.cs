using Quick.Protocol.Utils;

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
            try
            {
                //清理
                Dispose();
                cts = new CancellationTokenSource();
                var token = cts.Token;

                var stream = await InnerConnectAsync().ConfigureAwait(false);
                //初始化网络
                InitQpPackageHandler_Stream(stream);

                IsConnected = true;

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
                    Answer = CryptographyUtils.ComputeMD5Hash(AuthenticateQuestion + Options.Password)
                }).ConfigureAwait(false);

                Options.OnAuthPassed();
                IsAuchenticated = true;

                var repHandShake = await SendCommand(new Commands.HandShake.Request()
                {
                    EnableCompress = Options.EnableCompress,
                    EnableEncrypt = Options.EnableEncrypt,
                    TransportTimeout = Options.TransportTimeout
                }, 5000, true).ConfigureAwait(false);

                //开始心跳
                if (Options.HeartBeatInterval > 0)
                {
                    //定时发送心跳包
                    _ = BeginHeartBeat(token);
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                Dispose();
                throw;
            }
        }

        public override void Dispose()
        {
            cts?.Cancel();
            cts = null;
            base.Dispose();
        }
    }
}
