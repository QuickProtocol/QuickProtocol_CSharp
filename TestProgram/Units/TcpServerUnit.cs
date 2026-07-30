using System.Net;
using System.Text;
using Quick.Protocol;

namespace TestProgram.Units;

public class TcpServerUnit : AbstractServerUnit
{
    public override string Name => "TCP server";

    protected override QpServerOptions GetServerOptions() => new Quick.Protocol.Tcp.QpTcpServerOptions()
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
}