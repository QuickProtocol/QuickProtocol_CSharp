using System.Diagnostics;
using Quick.Protocol.Streams;
using Quick.Utils;

namespace TestProgram.Units;

public class ProcessCommunicateUnit:IUnit
{
    public string Name => "Process Communicate";

    public static void InvokeChildProcess()
    {
        Action<string> logger = t =>
        {
            File.AppendAllLines("ChildProcess.log", [t]);
        };
        var isDisconnected = false;
        var options = new QpStreamClientOptions()
        {
            Password = "HelloQP",
            EnableCompress = true,
            EnableEncrypt = true,
            BaseStream = new InputOutputStream(Console.OpenStandardInput(), Console.OpenStandardOutput()),
            Logger = new Quick.Protocol.QpLogger(logger)
            {
                LogConnection = true,
                LogCommand = true,
                LogPackage = true,
                LogContent=true,
                LogRaw = true
            }
        };
        var client = new QpStreamClient(options);
        client.Disconnected += (sender, e) =>
        {
            logger.Invoke("Disconnected.");
            isDisconnected = true;
        };
        logger.Invoke("Connecting...");
        client.ConnectAsync().ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                isDisconnected = true;
                logger.Invoke("Connect cancelled.");
                return;
            }
            if (t.IsFaulted)
            {
                isDisconnected = true;
                logger.Invoke("Connect error.Reason: " + ExceptionUtils.GetExceptionString(t.Exception));
                return;
            }
            logger.Invoke("Connected.");

            client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "Hello Quick.Protocol V2!" });
            //client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "".PadRight(5 * 1024, '0') });
        });
        while (!isDisconnected)
        {
            Thread.Sleep(1000);
        }
    }

    public void Invoke()
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
        channel.Disconnected += (sender, e) =>
        {
            Console.WriteLine(DateTime.Now.ToString() + "[Server]: disconnected");
        };
        channel.HeartbeatPackageReceived += (sender, e) => Console.WriteLine(DateTime.Now.ToString() + "[Server]: Heartbeat recved.");
        channel.RawNoticePackageReceived += (sender, e) => Console.WriteLine($"[Client_RawNoticePackageReceived]TypeName:{e.TypeName},Content:{e.Content}"); ;
        
        process.WaitForExit();
        Console.WriteLine($"Child process[{process.Id}] exited.");
        Console.ReadLine();
    }
}