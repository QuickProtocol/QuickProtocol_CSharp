using System.Diagnostics;
using Quick.Protocol;

namespace TestProgram.Units;

public class PerformanceTestUnit : AbstractServerUnit
{
    public override string Name => "Performance test";

    protected override QpServerOptions GetServerOptions() => new Quick.Protocol.Pipeline.QpPipelineServerOptions()
    {
        PipeName = "Quick.Protocol",
        Password = "HelloQP",
        ServerProgram = nameof(PipelineServerUnit) + " 1.0"
    };

    protected QpClientOptions GetClientOptions() => new Quick.Protocol.Pipeline.QpPipelineClientOptions()
    {
        PipeName = "Quick.Protocol",
        Password = "HelloQP",
        EnableCompress = true,
        EnableEncrypt = true
    };

    public override void Invoke()
    {
        var totalConnected = 0;
        var totalAuthed = 0;
        var totalDisconnected = 0;
        //启动服务端
        var serverOptions = GetServerOptions();
        var server = serverOptions.CreateServer();
        server.ChannelConnected += (_,_)=>
        {
            Interlocked.Add(ref totalConnected,1);
        };
        server.ChannelAuchenticated +=(_,_)=>
        {
            Interlocked.Add(ref totalAuthed,1);
        };
        server.ChannelDisconnected += (_,_)=>
        {
            Interlocked.Add(ref totalDisconnected,1);
        };
        server.Start();

        var process = Process.GetCurrentProcess();
        var clientOptions = GetClientOptions();
        while (true)
        {
            Thread.Sleep(1000);
            process.Refresh();
            Console.WriteLine($"[{DateTime.Now:T}]Current:{server.Channels.Length},Connected:{totalConnected},Authed:{totalAuthed},Disconnected:{totalDisconnected},Memory:{process.WorkingSet64}");
            for (var i = 0; i < 1000; i++)
            {
                Task.Run(async () =>
                {
                    using (var client = clientOptions.CreateClient())
                    {
                        await client.ConnectAsync();
                        await Task.Delay(Random.Shared.Next(100, 800));
                        client.Dispose();
                    }
                });
            }
        }
    }
}