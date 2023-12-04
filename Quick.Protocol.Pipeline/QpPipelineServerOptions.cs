using System;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Pipeline
{
    [JsonSerializable(typeof(QpPipelineServerOptions))]
    internal partial class QpPipelineServerOptionsOptionsSerializerContext : JsonSerializerContext { }

    public class QpPipelineServerOptions : QpServerOptions
    {
        protected override JsonSerializerContext JsonSerializerContext => QpPipelineServerOptionsOptionsSerializerContext.Default;

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
