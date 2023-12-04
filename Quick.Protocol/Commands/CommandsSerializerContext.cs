using System.Text.Json.Serialization;

namespace Quick.Protocol.Commands;

[JsonSerializable(typeof(Connect.Request))]
[JsonSerializable(typeof(Connect.Response))]
internal partial class ConnectCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Authenticate.Request))]
[JsonSerializable(typeof(Authenticate.Response))]
internal partial class AuthenticateCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(HandShake.Request))]
[JsonSerializable(typeof(HandShake.Response))]
internal partial class HandShakeCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(PrivateCommand.Request))]
[JsonSerializable(typeof(PrivateCommand.Response))]
internal partial class PrivateCommandCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetQpInstructions.Request))]
[JsonSerializable(typeof(GetQpInstructions.Response))]
internal partial class GetQpInstructionsCommandSerializerContext : JsonSerializerContext { }
