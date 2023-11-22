using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Pipeline
{
    [JsonSerializable(typeof(QpPipelineServerOptions))]
    internal partial class QpPipelineServerOptionsSerializerContext : JsonSerializerContext { }

    public class QpPipelineServerOptions : QpServerOptions
    {
        protected override JsonTypeInfo TypeInfo => QpPipelineServerOptionsSerializerContext.Default.QpPipelineServerOptions;

        public string PipeName { get; set; }

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(PipeName))
                throw new ArgumentNullException(nameof(PipeName));
        }

        public override QpServer CreateServer()
        {
            return new QpPipelineServer(this);
        }
    }
}
