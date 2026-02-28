using Microsoft.AspNetCore.Builder;
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
        private QpWebSocketServerOptions webSocketServerOptions;
        private QpWebSocketServer webSocketServer;

        public QpInterfaceServiceContext(QpInterfaceServiceContextOptions options)
        {
            this.options = options;
            if (options.Config.WebSocketEnable)
            {
                webSocketServerOptions = new QpWebSocketServerOptions()
                {
                    Path = options.Config.WebSocketPath,
                    InstructionSet = options.InstructionSet
                };
                options.WebBuilder.UseQuickProtocol(webSocketServerOptions, out webSocketServer);
                options.WebSocketServer = webSocketServer;
                options.WebSocketServerOptions = webSocketServerOptions;
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
            return list.ToArray();
        }

        public void Start()
        {
            if (options.Config.PipeEnable)
                pipeInterface = new PipeInterface(options);
            if (options.Config.TcpEnable)
                tcpInterface = new TcpInterface(options);
            if (options.Config.WebSocketEnable)
                webSocketInterface = new WebSocketInterface(options);

            pipeInterface?.Start();
            tcpInterface?.Start();
            webSocketInterface?.Start();
        }

        public void Stop()
        {
            pipeInterface?.Stop();
            pipeInterface = null;
            tcpInterface?.Stop();
            tcpInterface = null;
            webSocketInterface?.Stop();
            webSocketInterface = null;
        }
    }
}
