using System.Text.Json.Serialization;

namespace Quick.Protocol;

[JsonSerializable(typeof(QpStdioClientOptions))]
internal partial class QpStdioClientOptionsSerializerContext : JsonSerializerContext { }

public class QpStdioClientOptions : QpClientOptions
{
    protected override JsonSerializerContext GetJsonSerializerContext() => QpStdioClientOptionsSerializerContext.Default;
    public const string URI_SCHEMA = "qp.stdio";

    public override QpClient CreateClient()
    {
        return new QpStdioClient(this);
    }

    protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
    {
        return $"{URI_SCHEMA}://.";
    }

    public static void RegisterUriSchema()
    {
        RegisterUriSchema(URI_SCHEMA, () => new QpStdioClientOptions());
    }

    public override QpClientOptions Clone()
    {
        throw new System.NotImplementedException();
    }

    public override void Serialize(Stream stream)
    {
        throw new System.NotImplementedException();
    }
}