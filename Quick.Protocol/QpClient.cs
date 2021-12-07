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

        /// <summary>
        /// 连接断开时
        /// </summary>
        public event EventHandler Disconnected;

        public QpClient(QpClientOptions options)
            : base(options)
        {
            options.Check();
            this.Options = options;
        }

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

            var stream = await InnerConnectAsync();
            IsConnected = true;

            //初始化网络
            InitQpPackageHandler_Stream(stream);

            //开始读取其他数据包
            BeginReadPackage(token);

            var repConnect = await SendCommand(new Commands.Connect.Request()
            {
                InstructionIds = Options.InstructionSet.Select(t => t.Id).ToArray()
            });

            //如果服务端使用的缓存大小与客户端不同，则设置缓存大小为与服务端同样的大小
            if (BufferSize != repConnect.BufferSize)
                ChangeBufferSize(repConnect.BufferSize);

            var repAuth = await SendCommand(new Commands.Authenticate.Request()
            {
                Answer = CryptographyUtils.ComputeMD5Hash(repConnect.Question + Options.Password)
            });

            var repHandShake = await SendCommand(new Commands.HandShake.Request()
            {
                EnableCompress = Options.EnableCompress,
                EnableEncrypt = Options.EnableEncrypt,
                TransportTimeout = Options.TransportTimeout
            }, 5000, () =>
            {
                Options.OnAuthPassed();
            });

            //开始心跳
            if (Options.HeartBeatInterval > 0)
            {
                //定时发送心跳包
                BeginHeartBeat(token);
            }
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
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
        }

        protected virtual void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;
                Disconnected?.Invoke(this, QpEventArgs.Empty);                
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            cancellAll();
            InitQpPackageHandler_Stream(null);
            Disconnect();            
        }
    }
}
