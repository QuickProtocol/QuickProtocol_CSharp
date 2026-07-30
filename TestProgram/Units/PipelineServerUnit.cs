using Quick.Protocol;

namespace TestProgram.Units;

public class PipelineServerUnit
{
    public static void Invoke()
    {
        var commandExecuterManager = new CommandExecuterManager();
        commandExecuterManager.Register<Quick.Protocol.Commands.PrivateCommand.Request, Quick.Protocol.Commands.PrivateCommand.Response>(
            (handler, req) =>
            {
                return new Quick.Protocol.Commands.PrivateCommand.Response()
                {
                    Content = req.Content
                };
            });
        var noticeHandlerManager = new NoticeHandlerManager();
        noticeHandlerManager.Register<Quick.Protocol.Notices.PrivateNotice>(
            (handler, notice) =>
            {
                Console.WriteLine($"Recv PrivateNotice: {notice.Serialize(notice)}");
            });
        var serverOptions = new Quick.Protocol.Pipeline.QpPipelineServerOptions()
        {
            PipeName = "Quick.Protocol",
            Password = "HelloQP",
            ServerProgram = nameof(PipelineServerUnit) + " 1.0"
        };
        serverOptions.RegisterCommandExecuterManager(commandExecuterManager);
        serverOptions.RegisterNoticeHandlerManager(noticeHandlerManager);

        var server = new Quick.Protocol.Pipeline.QpPipelineServer(serverOptions);

        server.ChannelConnected += Server_ChannelConnected;
        server.ChannelDisconnected += Server_ChannelDisconnected;
        try
        {
            server.Start();
            Console.WriteLine($"Server started on pipe {serverOptions.PipeName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server start failed," + ex.ToString());
        }
        Console.ReadLine();
        server.Stop();
    }

    private static void Server_ChannelConnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] connected.");
    }


    private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] disconnected.");
    }
}