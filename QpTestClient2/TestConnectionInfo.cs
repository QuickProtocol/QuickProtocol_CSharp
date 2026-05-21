using Quick.Protocol;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QpTestClient
{
    [JsonSerializable(typeof(TestConnectionInfo))]
    [JsonSourceGenerationOptions]
    public partial class TestConnectionInfoSerializerContext : JsonSerializerContext
    {
        public static TestConnectionInfoSerializerContext Default2 { get; } = new TestConnectionInfoSerializerContext(new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public class TestConnectionInfo
    {
        public string Name { get; set; }
        public string QpClientTypeName { get; set; }
        public QpInstruction[] Instructions { get; set; }
        [JsonIgnore]
        public QpClientOptions QpClientOptions { get; set; }
    }
}
