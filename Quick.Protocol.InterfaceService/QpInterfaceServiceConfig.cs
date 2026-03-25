using System.Text.Json.Serialization;
using Quick.Fields;
using Quick.Protocol.JsonConverters;

namespace Quick.Protocol.InterfaceService;

public class QpInterfaceServiceConfig
{
    public WebSocket.Server.AspNetCore.QpWebSocketServerOptions WebSocketServerOptions { get; set; }
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool EnableWebSocket { get; set; }

    public Pipeline.QpPipelineServerOptions PipelineServerOptions { get; set; }
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool EnablePipeline{ get; set; }

    public Tcp.QpTcpServerOptions TcpServerOptions { get; set; }
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool EnableTcp{ get; set; }
    public Http.Server.AspNetCore.QpHttpServerOptions HttpServerOptions { get; set; }
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool EnableHttp{ get; set; }

    public static bool CanEnableWebSocket { get; set; } = true;
    public static bool CanEnablePipeline { get; set; } = true;
    public static bool CanEnableTcp { get; set; } = true;
    public static bool CanEnableHttp { get; set; } = true;

    private FieldForGet GetCommonConfigGroup(bool isReadOnly)
    {
        var list = new List<FieldForGet>();
        if (CanEnableWebSocket)
        {
            list.Add(new()
            {
                Id = nameof(EnableWebSocket),
                Name = "启用WebSocket",
                Input_AllowBlank = false,
                Type = FieldType.InputCheckbox,
                Value = EnableWebSocket.ToString(),
                PostOnChanged = true,
                Input_ReadOnly = isReadOnly
            });
        }
        if (CanEnablePipeline)
        {
            list.Add(new()
            {
                Id = nameof(EnablePipeline),
                Name = "启用管道",
                Input_AllowBlank = false,
                Type = FieldType.InputCheckbox,
                Value = EnablePipeline.ToString(),
                PostOnChanged = true,
                Input_ReadOnly = isReadOnly
            });
        }
        if (CanEnableTcp)
        {
            list.Add(new()
            {
                Id = nameof(EnableTcp),
                Name = "启用TCP",
                Input_AllowBlank = false,
                Type = FieldType.InputCheckbox,
                Value = EnableTcp.ToString(),
                PostOnChanged = true,
                Input_ReadOnly = isReadOnly
            });
        }
        if (CanEnableHttp)
        {
            list.Add(new()
            {
                Id = nameof(EnableHttp),
                Name = "启用HTTP",
                Input_AllowBlank = false,
                Type = FieldType.InputCheckbox,
                Value = EnableHttp.ToString(),
                PostOnChanged = true,
                Input_ReadOnly = isReadOnly
            });
        }
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "通用",
            Children = list.ToArray()
        };
    }

    public FieldForGet GetWebSocketConfigGroup(bool isReadOnly, QpInterfaceServiceConfig defaultModel)
    {
        var password = WebSocketServerOptions?.Password ?? defaultModel?.WebSocketServerOptions?.Password;
        var path = WebSocketServerOptions?.Path ?? defaultModel?.WebSocketServerOptions?.Path;
        return new FieldForGet()
        {
            Id = nameof(WebSocketServerOptions),
            Type = FieldType.ContainerGroup,
            Name = "WebSocket",
            Children =
            [
                new()
                {
                    Id = nameof(WebSocketServerOptions.Password),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputPassword,
                    Value = password,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(WebSocketServerOptions.Path),
                    Name = "WebSocket路径",
                    Description = "接口地址示例：qp.ws://127.0.0.1:8094" + path,
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = path,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    public FieldForGet GetPipelineConfigGroup(bool isReadOnly, QpInterfaceServiceConfig defaultModel)
    {
        var password = PipelineServerOptions?.Password ?? defaultModel?.PipelineServerOptions?.Password;
        var pipeName = PipelineServerOptions?.PipeName ?? defaultModel?.PipelineServerOptions?.PipeName;
        return new FieldForGet()
        {
            Id = nameof(PipelineServerOptions),
            Type = FieldType.ContainerGroup,
            Name = "管道",
            Children =
            [
                new()
                {
                    Id = nameof(PipelineServerOptions.Password),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputPassword,
                    Value = password,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(PipelineServerOptions.PipeName),
                    Name = "管道名称",
                    Description = $"接口地址示例：qp.pipe://./{pipeName}",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = pipeName,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    public FieldForGet GetTcpConfigGroup(bool isReadOnly, QpInterfaceServiceConfig defaultModel)
    {
        var password = TcpServerOptions?.Password ?? defaultModel?.TcpServerOptions?.Password;
        var address = TcpServerOptions?.Address ?? defaultModel?.TcpServerOptions?.Address;
        var port = TcpServerOptions?.Port ?? defaultModel?.TcpServerOptions?.Port;
        return new FieldForGet()
        {
            Id = nameof(TcpServerOptions),
            Type = FieldType.ContainerGroup,
            Name = "TCP",
            Children =
            [
                new()
                {
                    Id = nameof(TcpServerOptions.Password),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputPassword,
                    Value = password,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(TcpServerOptions.Address),
                    Name = "地址",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = address,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(TcpServerOptions.Port),
                    Name = "端口",
                    Description = $"接口地址示例：qp.tcp://127.0.0.1:{port}",
                    Input_AllowBlank = false,
                    Type = FieldType.InputNumber,
                    Value = port.ToString(),
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    public FieldForGet GetHttpConfigGroup(bool isReadOnly, QpInterfaceServiceConfig defaultModel)
    {
        var password = HttpServerOptions?.Password ?? defaultModel?.HttpServerOptions?.Password;
        var path = HttpServerOptions?.Path ?? defaultModel?.HttpServerOptions?.Path;
        return new FieldForGet()
        {
            Id = nameof(HttpServerOptions),
            Type = FieldType.ContainerGroup,
            Name = "HTTP",
            Children =
            [
                new()
                {
                    Id = nameof(HttpServerOptions.Password),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputPassword,
                    Value = password,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(HttpServerOptions.Path),
                    Name = "Http路径",
                    Description = "接口地址示例：qp.http://127.0.0.1:8094" + path,
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = path,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    public FieldForGet GetConfigGroup(FieldsForPostContainer request, bool isReadOnly, string id, string name, QpInterfaceServiceConfig defaultModel)
    {
        var list = new List<FieldForGet>()
        {
            GetCommonConfigGroup(isReadOnly)
        };
        if (CanEnableWebSocket && EnableWebSocket)
            list.Add(GetWebSocketConfigGroup(isReadOnly, defaultModel));
        if (CanEnablePipeline && EnablePipeline)
            list.Add(GetPipelineConfigGroup(isReadOnly, defaultModel));
        if (CanEnableTcp && EnableTcp)
            list.Add(GetTcpConfigGroup(isReadOnly, defaultModel));
        if (CanEnableHttp && EnableHttp)
            list.Add(GetHttpConfigGroup(isReadOnly, defaultModel));

        return new FieldForGet()
        {
            Id = id,
            Name = name,
            Type = FieldType.ContainerGroup,
            Children =
            [
                new()
                {
                    Type = FieldType.ContainerTab,
                    Children = list.ToArray()
                }
            ]
        };
    }
}
