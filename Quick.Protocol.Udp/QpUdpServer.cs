using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Udp
{
    public class QpUdpServer : QpServer
    {
        private UdpClient udpServer;
        private QpUdpServerOptions options;
        public IPEndPoint ListenEndPoint { get; private set; }
        public QpUdpServer(QpUdpServerOptions options) : base(options)
        {
            this.options = options;
        }

        public override void Start()
        {
            ListenEndPoint = new IPEndPoint(options.Address, options.Port);
            udpServer = new UdpClient(ListenEndPoint);
            base.Start();
        }

        public override void Stop()
        {
            udpServer?.Close();
            udpServer?.Dispose();
            udpServer = null;
            base.Stop();
        }

        protected override Task InnerAcceptAsync(CancellationToken token)
        {
            return udpServer.ReceiveAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                if (task.IsFaulted)
                    return;
                var ret = task.Result;
                var remoteEndPoint = ret.RemoteEndPoint;
                var buffer = ret.Buffer;

                try
                {
                    var remoteEndPointStr = "UDP:" + remoteEndPoint.ToString();
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]{0} connected.", remoteEndPointStr);
                    OnNewChannelConnected(new UdpClientStream(udpServer, remoteEndPoint, buffer), remoteEndPointStr, token);
                }
                catch (Exception ex)
                {
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]Init&Start Channel error,reason:{0}", ex.ToString());
                }
            });
        }
    }
}
