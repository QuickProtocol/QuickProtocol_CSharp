using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public Stream GetStream() => new Streams.InputOutputStream(Process.StandardOutput.BaseStream, Process.StandardInput.BaseStream);

    [JsonIgnore]
    public Process Process { get; set; }

    public string ChannelName => $"qp.stdio://{Process.Id}";

    public CancellationToken CancellationToken { get; set; }
}
