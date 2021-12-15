using Newtonsoft.Json.Linq;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace QpTestClient
{
    public class TestConnectionInfo
    {
        public string Name { get; set; }
        public string QpClientTypeName { get; set; }
        public QpClientOptions QpClientOptions { get; set; }
        public QpInstruction[] Instructions { get; set; }
    }
}
