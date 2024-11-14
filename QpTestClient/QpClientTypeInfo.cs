using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace QpTestClient
{
    public class QpClientTypeInfo
    {
        public string Name { get; set; }
        public Type ClientType { get; set; }
        public Func<QpClientOptions> CreateOptionsInstanceFunc { get; set; }
        public Func<Control> CreateOptionsControlFunc { get; set; }
        public override string ToString() => Name;
    }
}
