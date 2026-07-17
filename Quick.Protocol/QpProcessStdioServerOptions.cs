using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quick.Protocol.JsonConverters;

namespace Quick.Protocol;

[JsonSerializable(typeof(QpProcessStdioServerOptions))]
internal partial class QpProcessStdioServerOptionsSerializerContext : JsonSerializerContext
{
    public static QpProcessStdioServerOptionsSerializerContext Default2 { get; } = new QpProcessStdioServerOptionsSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

public class QpProcessStdioServerOptions : QpServerOptions
{
    protected override JsonSerializerContext GetJsonSerializerContext() => QpProcessStdioServerOptionsSerializerContext.Default2;
    public Stream GetStream()
    {
        var process = Process.GetProcessById(PID);
        return new Streams.InputOutputStream(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
    }

    [JsonConverter(typeof(QpJsonInt32Converter))]
    public int PID { get; set; }

    public string ChannelName => $"qp.stdio://{PID}";

    public CancellationToken CancellationToken { get; set; }
}
