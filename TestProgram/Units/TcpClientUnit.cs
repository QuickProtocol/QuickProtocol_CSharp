namespace TestProgram.Units;

public class TcpClientUnit
{
    public static void Invoke()
    {
        var client = new Quick.Protocol.Tcp.QpTcpClient(new Quick.Protocol.Tcp.QpTcpClientOptions()
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
            Console.WriteLine("Connection disconnected.");
        };
        client.ConnectAsync().ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                Console.WriteLine("Connect canceled.");
                return;
            }
            if (t.IsFaulted)
            {
                Console.WriteLine("Connect error," + t.Exception.InnerException.ToString());
                return;
            }
            Console.WriteLine("Connected");
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