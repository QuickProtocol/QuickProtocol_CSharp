using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.AllClients
{
    public class ConnectionUriParser
    {
        public static Uri GenerateConnectionUri(QpClientOptions options, bool includePassword = false, bool includeOtherProperty = false)
        {
            string baseUrl = "null://none/nothing";
            HashSet<string> ignorePropertyNames = new HashSet<string>();
            ignorePropertyNames.Add(nameof(options.HeartBeatInterval));
            if (!includePassword)
                ignorePropertyNames.Add(nameof(options.Password));

            if (options is Tcp.QpTcpClientOptions)
            {
                var item = (Tcp.QpTcpClientOptions)options;
                baseUrl=$"tcp://{item.Host}:{item.Port}";
                ignorePropertyNames.Add(nameof(item.Host));
                ignorePropertyNames.Add(nameof(item.Port));
            }
            else if (options is WebSocket.Client.QpWebSocketClientOptions)
            {
                var item = (WebSocket.Client.QpWebSocketClientOptions)options;
                baseUrl=item.Url;
                ignorePropertyNames.Add(nameof(item.Url));
            }
            else if (options is Pipeline.QpPipelineClientOptions)
            {
                var item = (Pipeline.QpPipelineClientOptions)options;
                baseUrl=$"pipe://{item.ServerName}/{item.PipeName}";
                ignorePropertyNames.Add(nameof(item.ServerName));
                ignorePropertyNames.Add(nameof(item.PipeName));
            }
            else if (options is SerialPort.QpSerialPortClientOptions)
            {
                var item = (SerialPort.QpSerialPortClientOptions)options;
                baseUrl=$"serialport://{item.PortName}";
                ignorePropertyNames.Add(nameof(item.PortName));
            }
            if (includePassword || includeOtherProperty)
            {
                StringBuilder sb = new StringBuilder(baseUrl);
                int currentIndex = 0;

                var jObj = JObject.FromObject(options);
                foreach (var property in jObj.Properties())
                {
                    var key = property.Name;
                    if (ignorePropertyNames.Contains(key))
                        continue;
                    if (!includeOtherProperty && key!=nameof(options.Password))
                        continue;
                    if (currentIndex==0)
                        sb.Append("?");
                    if (currentIndex>0)
                        sb.Append("&");
                    currentIndex++;

                    var value = property.Value.ToString();
                    value=System.Web.HttpUtility.UrlEncode(value);
                    sb.Append($"{key}={value}");
                }
                baseUrl=sb.ToString();
            }
            Uri uri = new Uri(baseUrl);
            return uri;
        }

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
            if (string.IsNullOrEmpty(uri.Query))
                return;
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
            options.ServerName= uri.Host;
            options.PipeName= uri.AbsolutePath.Replace("/", string.Empty);
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
