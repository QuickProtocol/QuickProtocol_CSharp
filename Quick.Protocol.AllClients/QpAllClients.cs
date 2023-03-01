using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol
{
    public class QpAllClients
    {
        public static void RegisterUriSchema()
        {
            Pipeline.QpPipelineClientOptions.RegisterUriSchema();
            Tcp.QpTcpClientOptions.RegisterUriSchema();
            Udp.QpUdpClientOptions.RegisterUriSchema();
            WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();
        }
    }
}
