using Quick.Protocol;
using Quick.Utils;

namespace TestProgram;

public abstract class AbstractServerUnit : IUnit
{
    public abstract string Name { get; }

    protected abstract QpServerOptions GetServerOptions();

    protected void StartServer(QpServer server)
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
        server.Options.RegisterCommandExecuterManager(commandExecuterManager);
        server.Options.RegisterNoticeHandlerManager(noticeHandlerManager);
        server.ChannelConnected += Server_ChannelConnected;
        server.ChannelDisconnected += Server_ChannelDisconnected;
        try
        {
            server.Start();
            Console.WriteLine($"Server started on {server.BindingPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server start failed." + ExceptionUtils.GetExceptionString(ex));
        }
    }

    public virtual void Invoke()
    {
        var options = GetServerOptions();        
        var server = options.CreateServer();
        StartServer(server);
        Console.ReadLine();
        server.Stop();
    }


    private void Server_ChannelConnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] connected.");
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    e.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice()
                    {
                        Action = "Time: ",
                        Content = DateTime.Now.ToString()
                    }).Wait();
                }
                catch { break; }
            }
        });
    }

    private void Server_ChannelDisconnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] disconnected.Reason: {ExceptionUtils.GetExceptionMessage(e.LastException)}");
    }
}