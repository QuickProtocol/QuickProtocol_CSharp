using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.WebSocket.Client
{
    [JsonSerializable(typeof(QpWebSocketClientOptions))]
    public partial class QpWebSocketClientOptionsSerializerContext : JsonSerializerContext
    {
        public static QpWebSocketClientOptionsSerializerContext Default2 { get; } = new QpWebSocketClientOptionsSerializerContext(new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public class QpWebSocketClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpWebSocketClientOptionsSerializerContext.Default2;

        public const string URI_SCHEMA_WS = "qp.ws";
        public const string URI_SCHEMA_WSS = "qp.wss";

        /// <summary>
        /// WebSocket的URL地址
        /// </summary>
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

        public override QpClientOptions Clone()
        {
            var json = JsonSerializer.Serialize(this, QpWebSocketClientOptionsSerializerContext.Default.QpWebSocketClientOptions);
            return JsonSerializer.Deserialize(json, QpWebSocketClientOptionsSerializerContext.Default.QpWebSocketClientOptions);
        }

        public override void Serialize(Stream stream)
        {
            JsonSerializer.Serialize(stream, this, QpWebSocketClientOptionsSerializerContext.Default.QpWebSocketClientOptions);
        }
    }
}
