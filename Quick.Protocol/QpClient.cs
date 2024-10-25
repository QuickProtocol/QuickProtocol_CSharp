using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol
{
    public abstract class QpClient : QpChannel
    {
        private CancellationTokenSource cts = null;
        public QpClientOptions Options { get; private set; }

        public QpClient(QpClientOptions options)
            : base(options)
        {
            options.Check();
            this.Options = options;
        }
        public override string ChannelName => Options.ToUri().ToString();
        protected abstract Task<Stream> InnerConnectAsync();

        /// <summary>
        /// 连接
        /// </summary>
        public async Task ConnectAsync()
        {
            //清理
            Close();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var stream = await InnerConnectAsync().ConfigureAwait(false);
            //初始化网络
            InitQpPackageHandler_Stream(stream);

            //开始读取其他数据包
            BeginReadPackage(token);
            //开始统计网络数据
            BeginNetstat(token);

            var repConnect = await SendCommand(new Commands.Connect.Request()
            {
                InstructionIds = Options.InstructionSet.Select(t => t.Id).ToArray()
            }).ConfigureAwait(false);
            AuthenticateQuestion = repConnect.Question;

            Options.OnAuthPassed();
            IsConnected = true;

            var repAuth = await SendCommand(new Commands.Authenticate.Request()
            {
                Answer = CryptographyUtils.ComputeMD5Hash(AuthenticateQuestion + Options.Password)
            }, 30000, true).ConfigureAwait(false);

            var repHandShake = await SendCommand(new Commands.HandShake.Request()
            {
                EnableCompress = Options.EnableCompress,
                EnableEncrypt = Options.EnableEncrypt,
                TransportTimeout = Options.TransportTimeout
            }, 5000).ConfigureAwait(false);

            //开始心跳
            if (Options.HeartBeatInterval > 0)
            {
                //定时发送心跳包
                BeginHeartBeat(token);
            }
        }
        protected override void OnWriteError(Exception exception)
        {
            base.OnWriteError(exception);
            Options.Init();
            cancellAll();
            Disconnect();
        }

        protected override void OnReadError(Exception exception)
        {
            base.OnReadError(exception);
            Options.Init();
            cancellAll();
            Disconnect();
        }

        private void cancellAll()
        {
            cts?.Cancel();
            cts = null;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            cancellAll();
            IsConnected = false;
            InitQpPackageHandler_Stream(null);
            Disconnect();
        }
    }
}
