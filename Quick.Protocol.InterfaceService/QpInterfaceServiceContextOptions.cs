using Microsoft.AspNetCore.Builder;
using Quick.Protocol.Http.Server.AspNetCore;
using Quick.Protocol.WebSocket.Server.AspNetCore;

namespace Quick.Protocol.InterfaceService
{
    public class QpInterfaceServiceContextOptions
    {
        public string InterfaceName { get; set; }
        public QpInstruction[] InstructionSet { get; set; }
        public IApplicationBuilder WebBuilder { get; set; }
        public QpWebSocketServer WebSocketServer { get; set; }
        public QpWebSocketServerOptions WebSocketServerOptions { get; set; }
        public QpHttpServer HttpServer { get; set; }
        public QpHttpServerOptions HttpServerOptions { get; set; }
        public CommandExecuterManager CommandExecuterManager { get; set; }
        public NoticeHandlerManager NoticeHandlerManager { get; set; }
        public Action<string> Logger { get; set; }
        public QpInterfaceServiceConfig Config { get; set; }
    }
}
