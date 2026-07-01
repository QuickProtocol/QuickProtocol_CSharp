using System.Diagnostics;
using Quick.Protocol.Streams;

namespace TestProgram.Units;

public class ProcessCommunicateUnit
{
    public static void InvokeChildProcess()
    {
        var isDisconnected = false;
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
        while (!isDisconnected)
        {
            Thread.Sleep(1000);
        }
    }

    public static void Invoke()
    {
        var psi = new ProcessStartInfo("dotnet");
        psi.ArgumentList.Add($"{nameof(TestProgram)}.dll");
        psi.ArgumentList.Add(nameof(InvokeChildProcess));
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardInput = true;
        var process = Process.Start(psi);
        Console.WriteLine($"Child process[{process.Id}] started.");
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
            Console.WriteLine(DateTime.Now.ToString() + "[Server]: auth timeout");
        };
        channel.Disconnected += (sender, e) =>
        {
            Console.WriteLine(DateTime.Now.ToString() + "[Server]: disconnected");
        };
        channel.HeartbeatPackageReceived += (sender, e) => Console.WriteLine(DateTime.Now.ToString() + "[Server]: Heartbeat recved.");
        channel.RawNoticePackageReceived += (sender, e) => Console.WriteLine($"[Client_RawNoticePackageReceived]TypeName:{e.TypeName},Content:{e.Content}"); ;
        
        process.WaitForExit();
    }
}