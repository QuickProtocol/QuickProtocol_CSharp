using System.Text.Json.Serialization;

namespace Quick.Protocol.Notices;

[JsonSerializable(typeof(PrivateNotice))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class NoticesSerializerContext : JsonSerializerContext { }
