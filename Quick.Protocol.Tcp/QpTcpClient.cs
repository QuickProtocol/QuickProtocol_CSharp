using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Tcp
{
    [DisplayName("TCP")]
    public class QpTcpClient : QpClient
    {
        private TcpClient tcpClient;
        private readonly QpTcpClientOptions options;

        public QpTcpClient(QpTcpClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            if (tcpClient != null)
                Dispose();
            //开始连接
            if (!string.IsNullOrEmpty(options.LocalHost) && options.LocalPort != null)
                tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(options.LocalHost), options.LocalPort.Value));
            else
                tcpClient = new TcpClient();

            using var cts = new CancellationTokenSource(options.ConnectionTimeout);
            try
            {
                await tcpClient.ConnectAsync(Dns.GetHostAddresses(options.Host), options.Port, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                tcpClient.Dispose();
                throw new TimeoutException($"Connection to {options.Host}:{options.Port} timed out.");
            }
            catch
            {
                tcpClient.Dispose();
                throw;
            }
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
