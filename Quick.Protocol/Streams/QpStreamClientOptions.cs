using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Streams
{
    [JsonSerializable(typeof(QpStreamClientOptions))]
    internal partial class QpStreamClientOptionsSerializerContext : JsonSerializerContext { }

    public class QpStreamClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpStreamClientOptionsSerializerContext.Default;
        [Browsable(false)]
        [JsonIgnore]
        public Stream BaseStream { get; set; }

        public override QpClient CreateClient()
        {
            return new QpStreamClient(this);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            return "qp.stream://" + BaseStream.GetType().Name;
        }

        public override QpClientOptions Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}
