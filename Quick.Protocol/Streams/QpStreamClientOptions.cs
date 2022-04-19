using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.Protocol.Streams
{
    public class QpStreamClientOptions : QpClientOptions
    {
        public Stream BaseStream { get; set; }

        public override Type GetQpClientType()
        {
            return typeof(QpStreamClient);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            return "stream://" + BaseStream.GetType().Name;
        }
    }
}
