using Microsoft.AspNetCore.Builder;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QpHttpServer.Core
{
    public class QpTestContext
    {
        private Quick.Protocol.Http.Server.AspNetCore.QpHttpServer server;

        public QpTestContext(IApplicationBuilder app)
        {
            Quick.Protocol.Utils.LogUtils.SetConsoleLogHandler();
            Quick.Protocol.Utils.LogUtils.LogPackage=true;
            Quick.Protocol.Utils.LogUtils.LogContent=true;
            Quick.Protocol.Utils.LogUtils.LogConnection = true;
            Quick.Protocol.Utils.LogUtils.LogHeartbeat = true;
            Quick.Protocol.Utils.LogUtils.LogCommand  =true;
            
            app.UseQuickProtocolHttp(new Quick.Protocol.Http.Server.AspNetCore.QpHttpServerOptions()
            {
                Path = "/qp_test",
                Password = "HelloQP",
                ServerProgram = nameof(QpHttpServer) + " 1.0",
                LongPollingTimeout = 2000,
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
