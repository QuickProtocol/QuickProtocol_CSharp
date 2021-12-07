using System;
using System.Net;
using System.Threading.Tasks;

namespace WebSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;
            Quick.Protocol.Utils.LogUtils.LogPackage = true;
            Quick.Protocol.Utils.LogUtils.LogHeartbeat = true;

            var client = new Quick.Protocol.WebSocket.Client.QpWebSocketClient(new Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions()
            {
                Url = "ws://127.0.0.1:3011/qp_test",
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
