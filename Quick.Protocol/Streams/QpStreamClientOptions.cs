using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Streams
{
    [JsonSerializable(typeof(QpStreamClientOptions))]
    internal partial class QpStreamClientOptionsSerializerContext : JsonSerializerContext { }

    public class QpStreamClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext JsonSerializerContext => QpStreamClientOptionsSerializerContext.Default;

        public Stream BaseStream { get; set; }

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
