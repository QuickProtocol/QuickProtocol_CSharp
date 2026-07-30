using Quick.Protocol;
using Quick.Utils;

namespace TestProgram;

public abstract class AbstractClientUnit : IUnit
{
    public abstract string Name { get; }

    protected abstract QpClientOptions GetClientOptions();
    public void Invoke()
    {
        var options = GetClientOptions();
        var client = options.CreateClient();
        client.RawNoticePackageReceived += Client_RawNoticePackageReceived;
        client.NoticePackageReceived += Client_NoticePackageReceived;
        client.Disconnected += (sender, e) =>
        {
            Console.WriteLine("Disconnected");
        };
        client.ConnectAsync().ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                Console.WriteLine("Connect canncelled.");
                return;
            }
            if (t.IsFaulted)
            {
                Console.WriteLine("Connect error," + ExceptionUtils.GetExceptionMessage(t.Exception));
                return;
            }
            Console.WriteLine("Connected");
        });
        Console.ReadLine();
    }


    private void Client_RawNoticePackageReceived(object sender, RawNoticePackageReceivedEventArgs e)
    {
        Console.WriteLine($"[Client_RawNoticePackageReceived]TypeName:{e.TypeName},Content:{e.Content}");
    }

    private void Client_NoticePackageReceived(object sender, NoticePackageReceivedEventArgs e)
    {
        Console.WriteLine($"[Client_NoticePackageReceived]TypeName:{e.TypeName},ContentModel:{e.ContentModel}");
    }
}