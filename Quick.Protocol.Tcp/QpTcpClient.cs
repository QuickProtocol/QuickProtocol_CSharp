using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Tcp
{
    [DisplayName("TCP")]
    public class QpTcpClient : QpClient
    {
        private TcpClient tcpClient;
        private QpTcpClientOptions options;

        public QpTcpClient(QpTcpClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            if (tcpClient != null)
                Close();
            //开始连接
            if (string.IsNullOrEmpty(options.LocalHost))
                tcpClient = new TcpClient();
            else
                tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(options.LocalHost), options.LocalPort));

            CancellationTokenSource cts = new CancellationTokenSource();
            var connectTask = tcpClient.ConnectAsync(Dns.GetHostAddresses(options.Host), options.Port, cts.Token).AsTask();
            try
            {
                await TaskUtils.TaskWait(connectTask, options.ConnectionTimeout)
                    .ConfigureAwait(false);
            }
            catch
            {
                cts.Cancel();
                tcpClient.Dispose();
                throw;
            }
            if (connectTask.IsFaulted)
                throw new IOException($"Failed to connect to {options.Host}:{options.Port}.", connectTask.Exception.InnerException);
            if (!tcpClient.Connected)
                throw new IOException($"Failed to connect to {options.Host}:{options.Port}.");
            return tcpClient.GetStream();
        }

        public override void Disconnect()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
                tcpClient = null;
            }

            base.Disconnect();
        }
    }
}
