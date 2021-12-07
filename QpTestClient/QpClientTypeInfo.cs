using System;
using System.Collections.Generic;
using System.Text;

namespace QpTestClient
{
    public class QpClientTypeInfo
    {
        public string Name { get; set; }
        public Type QpClientType { get; set; }
        public Type QpClientOptionsType { get; set; }
        public override string ToString() => Name;
    }
}
