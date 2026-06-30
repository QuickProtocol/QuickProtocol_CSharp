using Microsoft.AspNetCore.Builder;
using Quick.Protocol;
using System;

namespace WebApplication1.Core
{
    public class QpTestContext
    {
        private Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServer server;

        public QpTestContext(IApplicationBuilder app)
        {            
            app.UseQuickProtocol(new Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServerOptions()
            {
                Path = "/qp_test",
                Password = "HelloQP",
                ServerProgram = nameof(WebApplication1) + " 1.0"
            }, out server);

            server.ChannelConnected += Server_ChannelConnected;
            server.ChannelDisconnected += Server_ChannelDisconnected;
            server.Start();
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
