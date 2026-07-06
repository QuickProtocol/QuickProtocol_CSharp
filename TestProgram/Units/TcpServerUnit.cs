using System.Net;
using System.Text;
using Quick.Protocol;
using Quick.Utils;

namespace TestProgram.Units;

public class TcpServerUnit
{
    public static void Invoke()
    {
        var options = new Quick.Protocol.Tcp.QpTcpServerOptions()
        {
            Address = IPAddress.Loopback.ToString(),
            Port = 3011,
            Password = "HelloQP",
            ServerProgram = nameof(TcpServerUnit) + " 1.0",
            ProtocolErrorHandler = (stream, readBuffer) =>
            {
                string message = $@"
<html>
    <head>
        <title>Quick.Protocol</title>
    </head>
    <body>
        <p>Welcome to use <b>Quick.Protocol</b>.</p>
        <p>Source Code:<a href=""https://github.com/QuickProtocol"">https://github.com/QuickProtocol</a></p>
        <p>Time:{DateTime.Now}</p>
    </body>
</html>";
                try
                {
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.WriteLine($@"HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Content-Length: {Encoding.UTF8.GetByteCount(message)}

{message}");
                    }
                }
                catch { }
            }
        };
        var server = new Quick.Protocol.Tcp.QpTcpServer(options);
        server.ChannelConnected += Server_ChannelConnected;
        server.ChannelDisconnected += Server_ChannelDisconnected;
        try
        {
            server.Start();
            Console.WriteLine($"Tcp server started on {options.Address}:{options.Port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tcp server start failed." + ex.ToString());
        }
        Console.ReadLine();
        server.Stop();
    }

    private static void Server_ChannelConnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] connected.");
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    e.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice()
                    {
                        Action = "Time: ",
                        Content = DateTime.Now.ToString()
                    }).Wait();
                }
                catch { break; }
            }
        });
    }

    private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] disconnected.Reason: {ExceptionUtils.GetExceptionMessage(e.LastException)}");
    }
}