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
        tcpListener?.Dispose();
        tcpListener = null;
        base.Stop();
    }

    protected override async Task InnerAcceptAsync(CancellationToken token)
    {
        var tcpClient = await tcpListener.AcceptTcpClientAsync();
        try
        {
            var remoteEndPointStr = $"{QpTcpClientOptions.URI_SCHEMA}://{tcpClient.Client.RemoteEndPoint}";
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
            try { tcpClient.Dispose(); }
            catch { }
        }
    }
}