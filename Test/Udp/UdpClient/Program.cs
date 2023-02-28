using System;

namespace UdpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;
            Quick.Protocol.Utils.LogUtils.LogPackage = true;
            Quick.Protocol.Utils.LogUtils.LogHeartbeat = true;
            Quick.Protocol.Utils.LogUtils.LogNotice = true;
            Quick.Protocol.Utils.LogUtils.LogSplit = true;
            Quick.Protocol.Utils.LogUtils.LogContent = true;
            Quick.Protocol.Utils.LogUtils.SetConsoleLogHandler();

            var client = new Quick.Protocol.Udp.QpUdpClient(new Quick.Protocol.Udp.QpUdpClientOptions()
            {
                Host = "127.0.0.1",
                Port = 3011,
                Password = "HelloQP",
                EnableCompress = true,
                EnableEncrypt = true
            });
            client.RawNoticePackageReceived += Client_RawNoticePackageReceived;
            client.NoticePackageReceived += Client_NoticePackageReceived;
            client.Disconnected += (sender, e) =>
            {
                Console.WriteLine("连接已断开");
            };
            client.ConnectAsync().ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    Console.WriteLine("连接已取消");
                    return;
                }
                if (t.IsFaulted)
                {
                    Console.WriteLine("连接出错，原因：" + t.Exception.InnerException.ToString());
                    return;
                }
                Console.WriteLine("连接成功");
                //_ = client.SendCommand(new Quick.Protocol.Commands.PrivateCommand.Request()
                //{
                //    Action = "ABC",
                //    Content = "123"
                //});
            });
            Console.ReadLine();
        }

        private static void Client_RawNoticePackageReceived(object sender, Quick.Protocol.RawNoticePackageReceivedEventArgs e)
        {
            Console.WriteLine($"[Client_RawNoticePackageReceived]TypeName:{e.TypeName},Content:{e.Content}");
        }

        private static void Client_NoticePackageReceived(object sender, Quick.Protocol.NoticePackageReceivedEventArgs e)
        {
            Console.WriteLine($"[Client_NoticePackageReceived]TypeName:{e.TypeName},ContentModel:{e.ContentModel}");
        }
    }
}
