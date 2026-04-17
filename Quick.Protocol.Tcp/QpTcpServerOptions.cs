using System;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quick.Protocol.JsonConverters;

namespace Quick.Protocol.Tcp
{
    [JsonSerializable(typeof(QpTcpServerOptions))]
    public partial class QpTcpServerOptionsOptionsSerializerContext : JsonSerializerContext
    {
        public static QpTcpServerOptionsOptionsSerializerContext Default2 { get; } = new QpTcpServerOptionsOptionsSerializerContext(new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public class QpTcpServerOptions : QpServerOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpTcpServerOptionsOptionsSerializerContext.Default2;
        /// <summary>
        /// IP地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        [JsonConverter(typeof(QpJsonInt32Converter))]
        public int Port { get; set; }

        public override void Check()
        {
            base.Check();
            if (Address == null)
                throw new ArgumentNullException(nameof(Address));
            if (!IPAddress.TryParse(Address, out _))
                throw new ArgumentException($"Address[{Address}] format error.", nameof(Address));
            if (Port < 0 || Port > 65535)
                throw new ArgumentException("Port must between 0 and 65535", nameof(Port));
        }

        public override QpServer CreateServer()
        {
            return new QpTcpServer(this);
        }
    }
}
