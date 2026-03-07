using System.Text.Json.Serialization;
using Quick.Fields;
using Quick.Protocol.InterfaceService.JsonConverters;

namespace Quick.Protocol.InterfaceService;

public class QpInterfaceServiceConfig
{
    private Dictionary<string, string> enableDisableDict = new Dictionary<string, string>()
    {
        [true.ToString()] = "启用",
        [false.ToString()] = "禁用"
    };

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

    public FieldForGet GetConfigGroup(bool isReadOnly, string id, string name, QpInterfaceServiceConfig defaultModel)
    {
        //WebSocket
        var webSocketPath = WebSocketPath ?? defaultModel.WebSocketPath;
        var pipeName = PipeName ?? defaultModel.PipeName;
        var tcpListenPort = TcpListenPort > 0 ? TcpListenPort : defaultModel.TcpListenPort;

        var list = new List<FieldForGet>
        {
            new()
            {
                Id = nameof(Password),
                Name = "密码",
                Description = "默认密码：123456",
                Input_AllowBlank = false,
                Type = FieldType.InputPassword,
                Value = Password,
                Input_ReadOnly = isReadOnly
            },
            new ()
            {
                Name = "WebSocket",
                Type = FieldType.ContainerGroup,
                MarginBottom = 1,
                Children =
                [
                    new()
                    {
                        Id = nameof(WebSocketEnable),
                        Name = "启用",
                        Description = WebSocketEnable ? "接口地址示例：qp.ws://127.0.0.1:8094" + webSocketPath : null,
                        Input_AllowBlank = false,
                        Type = FieldType.InputSelect,
                        Value = WebSocketEnable.ToString(),
                        PostOnChanged = true,
                        InputSelect_Options = enableDisableDict,
                        Input_ReadOnly = isReadOnly
                    },
                    new()
                    {
                        Id = nameof(WebSocketPath),
                        Name = "WebSocket路径",
                        Input_AllowBlank = false,
                        Type = WebSocketEnable? FieldType.InputText: FieldType.InputHidden,
                        Value = webSocketPath,
                        Input_ReadOnly = isReadOnly
                    }
                ]
            },
            new ()
            {
                Name = "管道",
                Type = FieldType.ContainerGroup,
                MarginBottom = 1,
                Children =
                [
                    new()
                    {
                        Id = nameof(PipeEnable),
                        Name = "启用",
                        Description = PipeEnable ? $"接口地址示例：qp.pipe://./{pipeName}" : null,
                        Input_AllowBlank = false,
                        Type = FieldType.InputSelect,
                        Value = PipeEnable.ToString(),
                        PostOnChanged = true,
                        InputSelect_Options = enableDisableDict,
                        Input_ReadOnly = isReadOnly
                    },
                    new()
                    {
                        Id = nameof(PipeName),
                        Name = "管道名称",
                        Description = "默认密码：123456",
                        Input_AllowBlank = false,
                        Type = PipeEnable? FieldType.InputText: FieldType.InputHidden,
                        Value = pipeName,
                        Input_ReadOnly = isReadOnly
                    }
                ]
            },
            new ()
            {
                Name = "TCP",
                Type = FieldType.ContainerGroup,
                MarginBottom = 1,
                Children =
                [
                    new()
                    {
                        Id = nameof(TcpEnable),
                        Name = "启用",
                        Description = TcpEnable ? $"接口地址示例：qp.tcp://127.0.0.1:{tcpListenPort}" : null,
                        Input_AllowBlank = false,
                        Type = FieldType.InputSelect,
                        Value = TcpEnable.ToString(),
                        PostOnChanged = true,
                        InputSelect_Options = enableDisableDict,
                        Input_ReadOnly = isReadOnly
                    },
                    new()
                    {
                        Id = nameof(TcpListenAddress),
                        Name = "主机",
                        Input_AllowBlank = false,
                        Type = TcpEnable? FieldType.InputText: FieldType.InputHidden,
                        Value = TcpListenAddress ?? defaultModel.TcpListenAddress,
                        Input_ReadOnly = isReadOnly
                    },
                    new()
                    {
                        Id = nameof(TcpListenPort),
                        Name = "端口",
                        Input_AllowBlank = false,
                        Type = TcpEnable? FieldType.InputNumber: FieldType.InputHidden,
                        Value = tcpListenPort.ToString(),
                        Input_ReadOnly = isReadOnly
                    }
                ]
            }
        };
        return new FieldForGet()
        {
            Id = id,
            Type = FieldType.ContainerGroup,
            Name = name,
            Children = list.ToArray()
        };
    }
}
