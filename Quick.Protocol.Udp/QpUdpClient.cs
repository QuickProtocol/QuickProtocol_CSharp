using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Protocol.Udp
{
    [DisplayName("UDP")]
    public class QpUdpClient : QpClient
    {
        private UdpClient udpClient;
        private QpUdpClientOptions options;

        public QpUdpClient(QpUdpClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            if (udpClient != null)
                Close();
            //开始连接
            if (string.IsNullOrEmpty(options.LocalHost))
                udpClient = new UdpClient();
            else
                udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(options.LocalHost), options.LocalPort));
            var remoteHostAddresses = await Dns.GetHostAddressesAsync(options.Host);
            var remoteEndPoint = new IPEndPoint(remoteHostAddresses.First(), options.Port);
            await TaskUtils.TaskWait(Task.Run(() => udpClient.Connect(remoteEndPoint)), options.ConnectionTimeout);
            return new UdpClientStream(udpClient, remoteEndPoint);
        }

        public override void Disconnect()
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
                udpClient = null;
            }

            base.Disconnect();
        }
    }
}
