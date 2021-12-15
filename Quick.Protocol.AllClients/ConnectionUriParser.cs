using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Quick.Protocol.AllClients
{
    public class ConnectionUriParser
    {
        public static ConnectionInfo Parse(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "tcp":
                    return parseTcpConnectionInfo(uri);
                case "ws":
                    return parseWebSocketConnectionInfo(uri);
                case "pipe":
                    return parsePipeConnectionInfo(uri);
                case "serialport":
                    return parseSerialPortConnectionInfo(uri);
                default:
                    return null;
            }
        }

        private static void queryStringToOptions(Uri uri, QpClientOptions options)
        {
            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
            JObject jObj = new JObject();
            foreach (var key in queryString.AllKeys)
            {
                jObj.Add(key, queryString[key]);
            }
            JsonConvert.PopulateObject(jObj.ToString(), options);
        }

        private static ConnectionInfo parseTcpConnectionInfo(Uri uri)
        {
            var options = new Tcp.QpTcpClientOptions();
            options.Host = uri.Host;
            options.Port = uri.Port;
            queryStringToOptions(uri, options);
            return new ConnectionInfo()
            {
                QpClientType= typeof(Tcp.QpTcpClient),
                QpClientOptions=options
            };
        }

        private static ConnectionInfo parseWebSocketConnectionInfo(Uri uri)
        {
            var options = new WebSocket.Client.QpWebSocketClientOptions();
            options.Url = uri.ToString();
            queryStringToOptions(uri, options);
            return new ConnectionInfo()
            {
                QpClientType= typeof(WebSocket.Client.QpWebSocketClient),
                QpClientOptions=options
            };
        }

        private static ConnectionInfo parsePipeConnectionInfo(Uri uri)
        {
            var options = new Pipeline.QpPipelineClientOptions();
            options.PipeName= uri.Host;
            queryStringToOptions(uri, options);
            return new ConnectionInfo()
            {
                QpClientType= typeof(Pipeline.QpPipelineClient),
                QpClientOptions=options
            };
        }

        private static ConnectionInfo parseSerialPortConnectionInfo(Uri uri)
        {
            var options = new SerialPort.QpSerialPortClientOptions();
            options.PortName = uri.Host;
            queryStringToOptions(uri, options);
            return new ConnectionInfo()
            {
                QpClientType= typeof(SerialPort.QpSerialPortClientOptions),
                QpClientOptions=options
            };
        }
    }
}
