using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Tcp;

public class QpTcpServer : QpServer
{
    private TcpListener tcpListener;
    private IPAddress address;
    private int port;

    public override string BindingPath => $"{QpTcpClientOptions.URI_SCHEMA}://{ListenEndPoint}";

    public EndPoint ListenEndPoint { get; private set; }
    public QpTcpServer(QpTcpServerOptions options) : base(options)
    {
        address = IPAddress.Parse(options.Address);
        port = options.Port;
    }

    public override void Start()
    {
        tcpListener = new TcpListener(address, port);
        tcpListener.Start();
        ListenEndPoint = tcpListener.LocalEndpoint;
        base.Start();
    }

    public override void Stop()
    {
        tcpListener?.Stop();
        tcpListener = null;
        base.Stop();
    }

    protected override Task InnerAcceptAsync(CancellationToken token)
    {
        return tcpListener.AcceptTcpClientAsync().ContinueWith(task =>
        {
            var tcpClient = task.Result;
            if (tcpClient == null)
                return;
            try
            {
                var remoteEndPointStr = "TCP:" + tcpClient.Client.RemoteEndPoint.ToString();
                if (Options.Logger is { LogConnection: true })
                    Console.WriteLine("[Connection]{0} connected.", remoteEndPointStr);
                OnNewChannelConnected(tcpClient.GetStream(), remoteEndPointStr, token);
            }
            catch (Exception ex)
            {
                if (Options.Logger is { LogConnection: true })
                    Console.WriteLine("[Connection]Init&Start Channel error,reason:{0}", ex.ToString());
                try { tcpClient.Close(); }
                catch { }
            }
        });
    }
}