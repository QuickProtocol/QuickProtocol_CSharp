using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.WebSocket.Client
{
    public class QpWebSocketClientOptions : QpClientOptions
    {
        public const string URI_SCHEMA_WS = "ws";
        public const string URI_SCHEMA_WSS = "wss";

        /// <summary>
        /// WebSocket的URL地址
        /// </summary>
        [DisplayName("WebSocket地址")]
        [Category("常用")]
        public string Url { get; set; } = "ws://127.0.0.1:3011/qp_test";

        public override void Check()
        {
            base.Check();
            if (Url == null)
                throw new ArgumentNullException(nameof(Url));
            if (!Url.StartsWith(URI_SCHEMA_WS + "://") && !Url.StartsWith(URI_SCHEMA_WSS + "://"))
                throw new ArgumentException("Url must start with ws:// or wss://", nameof(Url));
        }

        public override Type GetQpClientType() => typeof(QpWebSocketClient);

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
            RegisterUriSchema(URI_SCHEMA_WS, typeof(QpWebSocketClientOptions));
            RegisterUriSchema(URI_SCHEMA_WSS, typeof(QpWebSocketClientOptions));
        }
    }
}
