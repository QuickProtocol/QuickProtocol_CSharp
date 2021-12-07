using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.Pipeline
{
    public class QpPipelineClientOptions : QpClientOptions
    {
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

        public override string GetConnectionInfo() => $"{ServerName}\\{PipeName}";
    }
}
