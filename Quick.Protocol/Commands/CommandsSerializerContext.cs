using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Commands;

[JsonSerializable(typeof(Connect.Request))]
[JsonSerializable(typeof(Connect.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ConnectCommandSerializerContext : JsonSerializerContext
{
    public static ConnectCommandSerializerContext Default2 { get; } = new ConnectCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

[JsonSerializable(typeof(Authenticate.Request))]
[JsonSerializable(typeof(Authenticate.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class AuthenticateCommandSerializerContext : JsonSerializerContext
{
    public static AuthenticateCommandSerializerContext Default2 { get; } = new AuthenticateCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

[JsonSerializable(typeof(HandShake.Request))]
[JsonSerializable(typeof(HandShake.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class HandShakeCommandSerializerContext : JsonSerializerContext
{
    public static HandShakeCommandSerializerContext Default2 { get; } = new HandShakeCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

[JsonSerializable(typeof(PrivateCommand.Request))]
[JsonSerializable(typeof(PrivateCommand.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class PrivateCommandCommandSerializerContext : JsonSerializerContext
{
    public static PrivateCommandCommandSerializerContext Default2 { get; } = new PrivateCommandCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

[JsonSerializable(typeof(GetQpInstructions.Request))]
[JsonSerializable(typeof(GetQpInstructions.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class GetQpInstructionsCommandSerializerContext : JsonSerializerContext
{
    public static GetQpInstructionsCommandSerializerContext Default2 { get; } = new GetQpInstructionsCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}
