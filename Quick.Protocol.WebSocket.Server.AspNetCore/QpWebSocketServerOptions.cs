using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.WebSocket.Server.AspNetCore
{
    [JsonSerializable(typeof(QpWebSocketServerOptions))]
    internal partial class QpWebSocketServerOptionsSerializerContext : JsonSerializerContext { }

    public class QpWebSocketServerOptions : QpServerOptions
    {
        protected override JsonTypeInfo TypeInfo => QpWebSocketServerOptionsSerializerContext.Default.QpWebSocketServerOptions;

        private string _Path;
        /// <summary>
        /// WebSocket的路径
        /// </summary>
        public string Path
        {
            get { return _Path; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _Path = "/";
                    return;
                }
                if (value.StartsWith("/"))
                {
                    _Path = value;
                    return;
                }
                _Path = "/" + value;
            }
        }

        public override QpServer CreateServer()
        {
            return new QpWebSocketServer(this);
        }
    }
}