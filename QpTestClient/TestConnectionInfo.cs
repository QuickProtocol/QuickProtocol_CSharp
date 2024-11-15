using Quick.Protocol;
using Quick.Protocol.SerialPort;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace QpTestClient
{
    [JsonSerializable(typeof(TestConnectionInfo))]
    public partial class TestConnectionInfoSerializerContext : JsonSerializerContext { }

    public class TestConnectionInfo
    {
        public string Name { get; set; }
        public string QpClientTypeName { get; set; }
        public QpInstruction[] Instructions { get; set; }
        [JsonIgnore]
        public QpClientOptions QpClientOptions { get; set; }
    }
}
