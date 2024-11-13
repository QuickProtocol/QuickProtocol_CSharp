using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace QpTestClient
{
    public class QpClientTypeInfo
    {
        public string Name { get; set; }
        public Type ClientType { get; set; }
        public Type OptionsType { get; set; }
        public Func<QpClientOptions> CreateOptionsInstanceFunc { get; set; }
        public override string ToString() => Name;
    }
}
