
namespace TestProgram.Units;

public class HttpClientUnit
{
    public static void Invoke()
    {
        var options = new Quick.Protocol.Http.Client.QpHttpClientOptions()
        {
            Url = "qp.http://127.0.0.1:3011/qp_test",
            Password = "HelloQP"
        };
        var client = options.CreateClient();
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
                Console.WriteLine("Connect error," + t.Exception.InnerException.ToString());
                return;
            }
            Console.WriteLine("Connected");
        });
        Console.ReadLine();
    }
}