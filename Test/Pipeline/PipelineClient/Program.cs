using System.Text.Json;
using System;

namespace PipelineClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;
            Quick.Protocol.Utils.LogUtils.LogPackage = true;
            //Quick.Protocol.Utils.LogUtils.LogHeartbeat = true;
            Quick.Protocol.Utils.LogUtils.LogNotice = true;
            //Quick.Protocol.Utils.LogUtils.LogSplit = true;
            Quick.Protocol.Utils.LogUtils.LogContent = true;

            var client = new Quick.Protocol.Pipeline.QpPipelineClient(new Quick.Protocol.Pipeline.QpPipelineClientOptions()
            {
                PipeName = "Quick.Protocol",
                Password = "HelloQP",
                EnableCompress = true,
                EnableEncrypt = true
            });

            client.Disconnected += (sender, e) =>
            {
                Console.WriteLine("连接已断开");
            };
            client.ConnectAsync().ContinueWith(async t =>
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

                try
                {
                    var rep = await client.SendCommand(
                        new Quick.Protocol.Commands.PrivateCommand.Request()
                        {
                            Action = "Echo",
                            Content = DateTime.Now.ToString()
                        });
                    Console.WriteLine("SendCommand Success.Rep:" + JsonSerializer.Serialize(rep));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendCommand Error:" + ex.ToString());
                }

                try
                {
                    await client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "Hello Quick.Protocol V2!" });
                    //await client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "".PadRight(5 * 1024, '0') });
                    Console.WriteLine("SendNotice Success.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendNotice Error:" + ex.ToString());
                }
            });
            Console.ReadLine();
        }
    }
}
