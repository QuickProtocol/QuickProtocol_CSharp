using System;

namespace QpHttpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Quick.Protocol.Http.Client.QpHttpClient(new Quick.Protocol.Http.Client.QpHttpClientOptions()
            {
                Url = "qp.http://127.0.0.1:3011/qp_test",
                Password = "HelloQP"
            });
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
            });
            Console.ReadLine();
        }
    }
}
