using System.Text.Json.Serialization;

namespace Quick.Protocol.Notices;

[JsonSerializable(typeof(PrivateNotice))]
internal partial class NoticesSerializerContext : JsonSerializerContext { }
