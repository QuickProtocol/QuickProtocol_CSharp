using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UdpAsTcp;

namespace Quick.Protocol.Udp
{
    public class QpUdpServer : QpServer
    {
        private UdpAsTcpListener udpAsTcpListener;
        private QpUdpServerOptions options;
        public QpUdpServer(QpUdpServerOptions options) : base(options)
        {
            this.options = options;
        }

        public override void Start()
        {
            udpAsTcpListener = new UdpAsTcpListener(new IPEndPoint(options.Address, options.Port));
            udpAsTcpListener.Start();
            base.Start();
        }

        public override void Stop()
        {
            udpAsTcpListener?.Stop();
            udpAsTcpListener = null;
            base.Stop();
        }

        protected override async Task InnerAcceptAsync(CancellationToken token)
        {
            var udpAsTcpClient = await udpAsTcpListener.AcceptClientAsync().ConfigureAwait(false);

            if (udpAsTcpClient == null)
                return;
            try
            {
                var remoteEndPointStr = "UDP:" + udpAsTcpClient.RemoteEndPoint.ToString();
                if (LogUtils.LogConnection)
                    LogUtils.Log("[Connection]{0} connected.", remoteEndPointStr);
                OnNewChannelConnected(udpAsTcpClient.GetStream(), remoteEndPointStr, token);
            }
            catch (Exception ex)
            {
                if (LogUtils.LogConnection)
                    LogUtils.Log("[Connection]Init&Start Channel error,reason:{0}", ex.ToString());
                try { udpAsTcpClient.Close(); }
                catch { }
            }
        }
    }
}
