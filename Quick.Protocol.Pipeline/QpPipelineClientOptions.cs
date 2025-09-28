using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Pipeline
{
    [JsonSerializable(typeof(QpPipelineClientOptions))]
    public partial class QpPipelineClientOptionsSerializerContext : JsonSerializerContext
    {
        public static QpPipelineClientOptionsSerializerContext Default2 { get; } = new QpPipelineClientOptionsSerializerContext(new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public class QpPipelineClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpPipelineClientOptionsSerializerContext.Default2;

        public const string URI_SCHEMA = "qp.pipe";

        public string ServerName { get; set; } = ".";

        public string PipeName { get; set; } = "Quick.Protocol";

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(PipeName))
                throw new ArgumentNullException(nameof(PipeName));
        }

        public override QpClient CreateClient()
        {
            return new QpPipelineClient(this);
        }

        protected override void LoadFromUri(Uri uri)
        {
            ServerName = uri.Host;
            PipeName = uri.AbsolutePath.Replace("/", string.Empty);
            base.LoadFromUri(uri);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            ignorePropertyNames.Add(nameof(ServerName));
            ignorePropertyNames.Add(nameof(PipeName));
            return $"{URI_SCHEMA}://{ServerName}/{PipeName}";
        }

        public static void RegisterUriSchema()
        {
            RegisterUriSchema(URI_SCHEMA, () => new QpPipelineClientOptions());
        }


        public override QpClientOptions Clone()
        {
            var json = JsonSerializer.Serialize(this, QpPipelineClientOptionsSerializerContext.Default.QpPipelineClientOptions);
            return JsonSerializer.Deserialize(json, QpPipelineClientOptionsSerializerContext.Default.QpPipelineClientOptions);
        }

        public override void Serialize(Stream stream)
        {
            JsonSerializer.Serialize(stream, this, QpPipelineClientOptionsSerializerContext.Default.QpPipelineClientOptions);
        }
    }
}
