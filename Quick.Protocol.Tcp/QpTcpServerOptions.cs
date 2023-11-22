using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Tcp
{
    [JsonSerializable(typeof(QpTcpServerOptions))]
    internal partial class DefaultSerializerContext : JsonSerializerContext { }

    public class QpTcpServerOptions : QpServerOptions
    {
        protected override JsonTypeInfo TypeInfo => DefaultSerializerContext.Default.QpTcpServerOptions;
        /// <summary>
        /// IP地址
        /// </summary>
        [JsonIgnore]
        public IPAddress Address { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        public override void Check()
        {
            base.Check();
            if (Address == null)
                throw new ArgumentNullException(nameof(Address));
            if (Port < 0 || Port > 65535)
                throw new ArgumentException("Port must between 0 and 65535", nameof(Port));
        }

        public override QpServer CreateServer()
        {
            return new QpTcpServer(this);
        }
    }
}
