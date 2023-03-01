using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UdpAsTcp;

namespace Quick.Protocol.Udp
{
    [DisplayName("UDP")]
    public class QpUdpClient : QpClient
    {
        private UdpAsTcpClient udpAsTcpClient;
        private QpUdpClientOptions options;

        public QpUdpClient(QpUdpClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            if (udpAsTcpClient != null)
                Close();
            //开始连接
            if (string.IsNullOrEmpty(options.LocalHost))
                udpAsTcpClient = new UdpAsTcpClient();
            else
                udpAsTcpClient = new UdpAsTcpClient(new IPEndPoint(IPAddress.Parse(options.LocalHost), options.LocalPort));
            await TaskUtils.TaskWait(Task.Run(() => udpAsTcpClient.Connect(options.Host, options.Port)), options.ConnectionTimeout);

            if (!udpAsTcpClient.Connected)
                throw new IOException($"Failed to connect to {options.Host}:{options.Port}.");
            return udpAsTcpClient.GetStream();
        }

        public override void Disconnect()
        {
            if (udpAsTcpClient != null)
            {
                udpAsTcpClient.Close();
                udpAsTcpClient.Dispose();
                udpAsTcpClient = null;
            }

            base.Disconnect();
        }
    }
}
