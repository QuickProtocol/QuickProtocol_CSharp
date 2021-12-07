using Quick.Protocol;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;
            Quick.Protocol.Utils.LogUtils.LogHeartbeat = true;
            //Quick.Protocol.Utils.LogUtils.LogPackage = true;
            //Quick.Protocol.Utils.LogUtils.LogContent = true;
            //Quick.Protocol.Utils.LogUtils.LogSplit = true;
            //Quick.Protocol.Utils.LogUtils.LogCommand = true;

            var server = new Quick.Protocol.Tcp.QpTcpServer(new Quick.Protocol.Tcp.QpTcpServerOptions()
            {
                Address = IPAddress.Loopback,
                Port = 3011,
                Password = "HelloQP",
                ServerProgram = nameof(TcpServer) + " 1.0",
                ProtocolErrorHandler = (stream, readBuffer) =>
                {
                    string message = $@"
<html>
    <head>
        <title>Quick.Protocol</title>
    </head>
    <body>
        <p>Welcome to use <b>Quick.Protocol</b>.</p>
        <p>Source Code:<a href=""http://github.com/aaasoft/Quick.Protocol"">http://github.com/aaasoft/Quick.Protocol</a></p>
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
            });
            server.ChannelConnected += Server_ChannelConnected;
            server.ChannelDisconnected += Server_ChannelDisconnected;
            try
            {
                server.Start();
                Console.WriteLine($"服务启动成功!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"服务启动失败!" + ex.ToString());
            }
            Console.ReadLine();
            server.Stop();
        }

        private static void Server_ChannelConnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已连接!");
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    e.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice()
                    {
                        Action = "NowTime",
                        Content = DateTime.Now.ToString()
                    });
                }
            });
        }

        private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已断开!");
        }
    }
}
