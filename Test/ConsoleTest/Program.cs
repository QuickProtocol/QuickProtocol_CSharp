using Quick.Protocol.Streams;
using System;
using System.Diagnostics;
using System.Threading;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var isDisconnected = false;

            if (args == null || args.Length == 0)
            {
                var psi = new ProcessStartInfo("dotnet");
                psi.ArgumentList.Add($"{nameof(ConsoleTest)}.dll");
                psi.ArgumentList.Add("client");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                var process = Process.Start(psi);
                Console.WriteLine($"子进程[{process.Id}]已启动。");
                var options = new QpStreamServerOptions()
                {
                    Password = "HelloQP",
                    BaseStream = new InputOutputStream(process.StandardOutput.BaseStream, process.StandardInput.BaseStream),
                    CancellationToken = CancellationToken.None,
                    ChannelName = $"Process:{process.Id}"
                };
                var channel = new QpStreamServerChannel(options);
                channel.AuchenticateTimeout += (sender, e) =>
                {
                    Console.WriteLine(DateTime.Now.ToString() + "[Server]: 认证超时！");
                    isDisconnected = true;
                };
                channel.Disconnected += (sender, e) =>
                {
                    Console.WriteLine(DateTime.Now.ToString() + "[Server]: 连接已断开！");
                    isDisconnected = true;
                };
                channel.HeartbeatPackageReceived += (sender, e) => Console.WriteLine(DateTime.Now.ToString() + "[Server]: 收到心跳包");
                channel.RawNoticePackageReceived+=(sender,e)=> Console.WriteLine($"[Client_RawNoticePackageReceived]TypeName:{e.TypeName},Content:{e.Content}"); ;                
            }
            else
            {
                var options = new QpStreamClientOptions()
                {
                    Password = "HelloQP",
                    EnableCompress = true,
                    EnableEncrypt = true,
                    BaseStream = new InputOutputStream(Console.OpenStandardInput(), Console.OpenStandardOutput())
                };
                var client = new QpStreamClient(options);
                client.Disconnected += (sender, e) =>
                {
                    Debug.WriteLine("连接已断开");
                    isDisconnected = true;
                };
                client.ConnectAsync().ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        isDisconnected = true;
                        Debug.WriteLine("连接已取消");
                        return;
                    }
                    if (t.IsFaulted)
                    {
                        isDisconnected = true;
                        Debug.WriteLine("连接出错，原因：" + t.Exception.InnerException.ToString());
                        return;
                    }
                    Debug.WriteLine("连接成功");

                    client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "Hello Quick.Protocol V2!" });
                    //client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "".PadRight(5 * 1024, '0') });
                });                
            }
            while (!isDisconnected)
            {
                Thread.Sleep(1000);
            }
        }
    }
}