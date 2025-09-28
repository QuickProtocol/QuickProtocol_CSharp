using System.ComponentModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Quick.Protocol.Streams
{
    [JsonSerializable(typeof(QpStreamServerOptions))]
    internal partial class QpStreamServerOptionsSerializerContext : JsonSerializerContext
    {
        public static QpStreamServerOptionsSerializerContext Default2 { get; } = new QpStreamServerOptionsSerializerContext(new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public class QpStreamServerOptions : QpServerOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpStreamServerOptionsSerializerContext.Default2;
        public Stream BaseStream { get; set; }
        public string ChannelName { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
