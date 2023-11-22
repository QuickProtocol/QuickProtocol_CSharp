using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Streams
{
    [JsonSerializable(typeof(QpStreamClientOptions))]
    internal partial class QpStreamClientOptionsSerializerContext : JsonSerializerContext { }

    public class QpStreamClientOptions : QpClientOptions
    {
        public Stream BaseStream { get; set; }

        protected override JsonTypeInfo TypeInfo => QpStreamClientOptionsSerializerContext.Default.QpStreamClientOptions;

        public override QpClient CreateClient()
        {
            return new QpStreamClient(this);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            return "qp.stream://" + BaseStream.GetType().Name;
        }
    }
}
