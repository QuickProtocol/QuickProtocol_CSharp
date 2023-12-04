using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace Quick.Protocol.WebSocket.Client
{
    [JsonSerializable(typeof(QpWebSocketClientOptions))]
    internal partial class QpWebSocketClientOptionsSerializerContext : JsonSerializerContext { }

    public class QpWebSocketClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpWebSocketClientOptionsSerializerContext.Default;

        public const string URI_SCHEMA_WS = "qp.ws";
        public const string URI_SCHEMA_WSS = "qp.wss";

        /// <summary>
        /// WebSocket的URL地址
        /// </summary>
        [DisplayName("WebSocket地址")]
        [Category("常用")]
        public string Url { get; set; } = "qp.ws://127.0.0.1:3011/qp_test";

        public override void Check()
        {
            base.Check();
            if (Url == null)
                throw new ArgumentNullException(nameof(Url));
            if (!Url.StartsWith(URI_SCHEMA_WS + "://") && !Url.StartsWith(URI_SCHEMA_WSS + "://"))
                throw new ArgumentException("Url must start with qp.ws:// or qp.wss://", nameof(Url));
        }

        public override QpClient CreateClient()
        {
            return new QpWebSocketClient(this);
        }

        protected override void LoadFromUri(Uri uri)
        {
            Url = uri.ToString();
            base.LoadFromUri(uri);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            ignorePropertyNames.Add(nameof(Url));
            return Url;
        }

        public static void RegisterUriSchema()
        {
            RegisterUriSchema(URI_SCHEMA_WS, () => new QpWebSocketClientOptions());
            RegisterUriSchema(URI_SCHEMA_WSS, () => new QpWebSocketClientOptions());
        }
    }
}
