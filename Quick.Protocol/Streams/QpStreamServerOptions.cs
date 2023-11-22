using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;

namespace Quick.Protocol.Streams
{
    [JsonSerializable(typeof(QpStreamServerOptions))]
    internal partial class QpStreamServerOptionsSerializerContext : JsonSerializerContext { }

    public class QpStreamServerOptions : QpServerOptions
    {
        protected override JsonTypeInfo TypeInfo => QpStreamServerOptionsSerializerContext.Default.QpStreamServerOptions;
        public Stream BaseStream { get; set; }
        public string ChannelName { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
