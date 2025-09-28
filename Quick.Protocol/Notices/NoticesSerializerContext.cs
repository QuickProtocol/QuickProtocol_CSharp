using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Notices;

[JsonSerializable(typeof(PrivateNotice))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class NoticesSerializerContext : JsonSerializerContext
{
    public static NoticesSerializerContext Default2 { get; } = new NoticesSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}
