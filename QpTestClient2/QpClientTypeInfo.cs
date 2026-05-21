using QpTestClient.Controls;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.IO;

namespace QpTestClient
{
    public class QpClientTypeInfo
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
        public Func<QpClientOptions> CreateOptionsInstanceFunc { get; set; }
        public Func<Stream, QpClientOptions> DeserializeQpClientOptions { get; set; }
        public Action<AotPropertyGrid, QpClientOptions> EditOptions { get; set; }
        public override string ToString() => Name;
    }
}
