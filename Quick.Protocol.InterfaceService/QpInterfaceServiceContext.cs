using Microsoft.AspNetCore.Builder;
using Quick.Protocol.Http.Server.AspNetCore;
using Quick.Protocol.InterfaceService.Interfaces;
using Quick.Protocol.WebSocket.Server.AspNetCore;

namespace Quick.Protocol.InterfaceService
{
    public class QpInterfaceServiceContext
    {
        private QpInterfaceServiceContextOptions options;
        private PipeInterface pipeInterface;
        private TcpInterface tcpInterface;
        private WebSocketInterface webSocketInterface;
        private HttpInterface httpInterface;

        private QpWebSocketServerOptions webSocketServerOptions;
        private QpWebSocketServer webSocketServer;
        private QpHttpServerOptions httpServerOptions;
        private QpHttpServer httpServer;

        public QpInterfaceServiceContext(QpInterfaceServiceContextOptions options)
        {
            this.options = options;
            if (options.Config.EnableWebSocket)
            {
                webSocketServerOptions = options.Config.WebSocketServerOptions;
                webSocketServerOptions.InstructionSet = options.InstructionSet;
                options.WebBuilder.UseQuickProtocol(webSocketServerOptions, out webSocketServer);
                options.WebSocketServer = webSocketServer;
                options.WebSocketServerOptions = webSocketServerOptions;
            }
            if (options.Config.EnableHttp)
            {
                httpServerOptions = options.Config.HttpServerOptions;
                httpServerOptions.InstructionSet = options.InstructionSet;
                options.WebBuilder.UseQuickProtocolHttp(httpServerOptions, out httpServer);
                options.HttpServer = httpServer;
                options.HttpServerOptions = httpServerOptions;
            }
        }

        public QpChannel[] GetAllChannels()
        {
            List<QpChannel> list = new List<QpChannel>();
            if (pipeInterface != null)
                list.AddRange(pipeInterface.GetAllChannels());
            if (tcpInterface != null)
                list.AddRange(tcpInterface.GetAllChannels());
            if (webSocketInterface != null)
                list.AddRange(webSocketInterface.GetAllChannels());
            if (httpInterface != null)
                list.AddRange(httpInterface.GetAllChannels());
            return list.ToArray();
        }

        public void Start()
        {
            if (options.Config.EnablePipeline)
                pipeInterface = new PipeInterface(options);
            if (options.Config.EnableTcp)
                tcpInterface = new TcpInterface(options);
            if (options.Config.EnableWebSocket)
                webSocketInterface = new WebSocketInterface(options);
            if (options.Config.EnableHttp)
                httpInterface = new HttpInterface(options);

            pipeInterface?.Start();
            tcpInterface?.Start();
            webSocketInterface?.Start();
            httpInterface?.Start();
        }

        public void Stop()
        {
            pipeInterface?.Stop();
            pipeInterface = null;
            tcpInterface?.Stop();
            tcpInterface = null;
            webSocketInterface?.Stop();
            webSocketInterface = null;
            httpInterface?.Stop();
            httpInterface = null;
        }
    }
}
