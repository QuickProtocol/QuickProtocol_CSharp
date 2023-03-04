using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.Protocol.Streams
{
    public class QpStreamClientOptions : QpClientOptions
    {
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
