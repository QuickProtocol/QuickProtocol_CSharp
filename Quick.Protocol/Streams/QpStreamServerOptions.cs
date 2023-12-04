using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;

namespace Quick.Protocol.Streams
{
    [JsonSerializable(typeof(QpStreamServerOptions))]
    internal partial class QpStreamServerOptionsSerializerContext : JsonSerializerContext { }

    public class QpStreamServerOptions : QpServerOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpStreamServerOptionsSerializerContext.Default;
        public Stream BaseStream { get; set; }
        public string ChannelName { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
