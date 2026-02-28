using System.Text.Json.Serialization;
using Quick.Protocol.InterfaceService.JsonConverters;

namespace Quick.Protocol.InterfaceService;

public class QpInterfaceServiceConfig
{
    public string Password { get; set; } = "123456";
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool WebSocketEnable { get; set; } = false;
    public string WebSocketPath { get; set; }
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool PipeEnable { get; set; } = true;
    public string PipeName { get; set; }
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool TcpEnable { get; set; } = false;
    public string TcpListenAddress { get; set; } = "0.0.0.0";
    [JsonConverter(typeof(QpJsonInt32Converter))]
    public int TcpListenPort { get; set; }
    [JsonConverter(typeof(QpJsonInt32Converter))]
    public int MaxPackageSize { get; set; } = 100 * 1024 * 1024;
}
