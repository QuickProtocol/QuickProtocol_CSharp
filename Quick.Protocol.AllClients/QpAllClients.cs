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
            SerialPort.QpSerialPortClientOptions.RegisterUriSchema();
            Tcp.QpTcpClientOptions.RegisterUriSchema();
            WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();
        }
    }
}
