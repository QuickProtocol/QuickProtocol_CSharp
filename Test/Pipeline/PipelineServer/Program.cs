using Quick.Protocol;
using System;

namespace PipelineServer
{
    class Program
    {
        static void Main(string[] args)
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
                    Console.WriteLine($"收到PrivateNotice: {notice.Serialize(notice)}");
                });
            var serverOptions = new Quick.Protocol.Pipeline.QpPipelineServerOptions()
            {
                PipeName = "Quick.Protocol",
                Password = "HelloQP",
                ServerProgram = nameof(PipelineServer) + " 1.0"
            };
            serverOptions.RegisterCommandExecuterManager(commandExecuterManager);
            serverOptions.RegisterNoticeHandlerManager(noticeHandlerManager);

            var server = new Quick.Protocol.Pipeline.QpPipelineServer(serverOptions);

            server.ChannelConnected += Server_ChannelConnected;
            server.ChannelDisconnected += Server_ChannelDisconnected;
            try
            {
                server.Start();
                Console.WriteLine($"服务启动成功!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"服务启动失败!" + ex.ToString());
            }
            Console.ReadLine();
            server.Stop();
        }

        private static void Server_ChannelConnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已连接!");
        }


        private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已断开!");
        }
    }
}
