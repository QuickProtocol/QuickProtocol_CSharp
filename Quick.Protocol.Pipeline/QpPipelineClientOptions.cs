using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.Pipeline
{
    public class QpPipelineClientOptions : QpClientOptions
    {
        public const string URI_SCHEMA = "qp.pipe";

        [Category("常用")]
        [DisplayName("服务器名称")]
        public string ServerName { get; set; } = ".";

        [Category("常用")]
        [DisplayName("管道名称")]
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
            ServerName =uri.Host;
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
    }
}
