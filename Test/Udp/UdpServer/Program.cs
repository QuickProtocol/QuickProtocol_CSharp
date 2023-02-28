using Quick.Protocol;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;
            Quick.Protocol.Utils.LogUtils.LogHeartbeat = true;
            Quick.Protocol.Utils.LogUtils.LogPackage = true;
            Quick.Protocol.Utils.LogUtils.LogContent = true;
            Quick.Protocol.Utils.LogUtils.LogSplit = true;
            Quick.Protocol.Utils.LogUtils.LogCommand = true;
            Quick.Protocol.Utils.LogUtils.SetConsoleLogHandler();

            var server = new Quick.Protocol.Udp.QpUdpServer(new Quick.Protocol.Udp.QpUdpServerOptions()
            {
                Address = IPAddress.Loopback,
                Port = 3011,
                Password = "HelloQP",
                ServerProgram = nameof(UdpServer) + " 1.0"
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
                    try
                    {
                        e.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice()
                        {
                            Action = "NowTime",
                            Content = DateTime.Now.ToString()
                        }).Wait();
                    }
                    catch { break; }
                }
            });
        }

        private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已断开!");
        }
    }
}
