using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Tcp
{
    [JsonSerializable(typeof(QpTcpClientOptions))]
    public partial class QpTcpClientOptionsSerializerContext : JsonSerializerContext { }

    public class QpTcpClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpTcpClientOptionsSerializerContext.Default;

        public const string URI_SCHEMA = "qp.tcp";
        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 3011;
        /// <summary>
        /// 本地主机
        /// </summary>
        public string LocalHost { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int? LocalPort { get; set; }

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(Host))
                throw new ArgumentNullException(nameof(Host));
            if (Port < 0 || Port > 65535)
                throw new ArgumentException("Port must between 0 and 65535", nameof(Port));
        }

        public override QpClient CreateClient()
        {
            return new QpTcpClient(this);
        }

        protected override void LoadFromQueryString(string key, string value)
        {
            switch (key)
            {
                case nameof(LocalHost):
                    LocalHost = value;
                    break;
                case nameof(LocalPort):
                    LocalPort = int.Parse(value);
                    break;
                default:
                    base.LoadFromQueryString(key, value);
                    break;
            }
        }

        protected override void LoadFromUri(Uri uri)
        {
            Host = uri.Host;
            Port = uri.Port;
            base.LoadFromUri(uri);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            ignorePropertyNames.Add(nameof(Host));
            ignorePropertyNames.Add(nameof(Port));
            return $"{URI_SCHEMA}://{Host}:{Port}";
        }

        public static void RegisterUriSchema()
        {
            RegisterUriSchema(URI_SCHEMA, () => new QpTcpClientOptions());
        }

        public override QpClientOptions Clone()
        {
            var json = JsonSerializer.Serialize(this, QpTcpClientOptionsSerializerContext.Default.QpTcpClientOptions);
            return JsonSerializer.Deserialize(json, QpTcpClientOptionsSerializerContext.Default.QpTcpClientOptions);
        }

        public override void Serialize(Stream stream)
        {
            JsonSerializer.Serialize(stream, this, QpTcpClientOptionsSerializerContext.Default.QpTcpClientOptions);
        }
    }
}
