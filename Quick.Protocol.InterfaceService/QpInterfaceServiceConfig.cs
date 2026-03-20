using System.Text.Json.Serialization;
using Quick.Fields;
using Quick.Protocol.JsonConverters;

namespace Quick.Protocol.InterfaceService;

public class QpInterfaceServiceConfig
{
    private Dictionary<string, string> enableDisableDict = new Dictionary<string, string>()
    {
        [true.ToString()] = "是",
        [false.ToString()] = "否"
    };

    public WebSocket.Server.AspNetCore.QpWebSocketServerOptions WebSocketServerOptions { get; set; }
    [JsonIgnore]
    public bool EnableWebSocket => CanEnableWebSocket && WebSocketServerOptions != null;

    public Pipeline.QpPipelineServerOptions PipelineServerOptions { get; set; }
    [JsonIgnore]
    public bool EnablePipeline => CanEnablePipeline && PipelineServerOptions != null;

    public Tcp.QpTcpServerOptions TcpServerOptions { get; set; }
    [JsonIgnore]
    public bool EnableTcp => CanEnableTcp && TcpServerOptions != null;
    public Http.Server.AspNetCore.QpHttpServerOptions HttpServerOptions { get; set; }
    [JsonIgnore]
    public bool EnableHttp => CanEnableHttp && HttpServerOptions != null;

    public static bool CanEnableWebSocket { get; set; } = true;
    public static bool CanEnablePipeline { get; set; } = true;
    public static bool CanEnableTcp { get; set; } = true;
    public static bool CanEnableHttp { get; set; } = true;

    public FieldForGet GetCommonConfigGroup(FieldsForPostContainer request, bool isReadOnly, string id, QpInterfaceServiceConfig defaultModel)
    {
        var list = new List<FieldForGet>();
        if (CanEnableWebSocket)
        {
            if(request!=null)
            {
                var currentEnable = bool.Parse(request.GetFieldValue(id, nameof(EnableWebSocket)));
                if (currentEnable != EnableWebSocket)
                    WebSocketServerOptions = currentEnable ? new() : null;
            }
            list.Add(new()
            {
                Id = nameof(EnableWebSocket),
                Name = "启用WebSocket",
                Input_AllowBlank = false,
                Type = FieldType.InputSelect,
                Value = EnableWebSocket.ToString(),
                PostOnChanged = true,
                InputSelect_Options = enableDisableDict,
                Input_ReadOnly = isReadOnly
            });
        }
        if (CanEnablePipeline)
        {
            if(request!=null)
            {
                var currentEnable = bool.Parse(request.GetFieldValue(id, nameof(EnablePipeline)));
                if (currentEnable != EnablePipeline)
                    PipelineServerOptions = currentEnable ? new() : null;
            }
            list.Add(new()
            {
                Id = nameof(EnablePipeline),
                Name = "启用管道",
                Input_AllowBlank = false,
                Type = FieldType.InputSelect,
                Value = EnablePipeline.ToString(),
                PostOnChanged = true,
                InputSelect_Options = enableDisableDict,
                Input_ReadOnly = isReadOnly
            });
        }
        if (CanEnableTcp)
        {
            if(request!=null)
            {
                var currentEnable = bool.Parse(request.GetFieldValue(id, nameof(EnableTcp)));
                if (currentEnable != EnableTcp)
                    TcpServerOptions = currentEnable ? new() : null;
            }
            list.Add(new()
            {
                Id = nameof(EnableTcp),
                Name = "启用TCP",
                Input_AllowBlank = false,
                Type = FieldType.InputSelect,
                Value = EnableTcp.ToString(),
                PostOnChanged = true,
                InputSelect_Options = enableDisableDict,
                Input_ReadOnly = isReadOnly
            });
        }
        if (CanEnableHttp)
        {
            if (request != null)
            {
                var currentEnable = bool.Parse(request.GetFieldValue(id, nameof(EnableHttp)));
                if (currentEnable != EnableHttp)
                    HttpServerOptions = currentEnable ? new() : null;
            }
            list.Add(new()
            {
                Id = nameof(EnableHttp),
                Name = "启用HTTP",
                Input_AllowBlank = false,
                Type = FieldType.InputSelect,
                Value = EnableHttp.ToString(),
                PostOnChanged = true,
                InputSelect_Options = enableDisableDict,
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
            GetCommonConfigGroup(request,isReadOnly,id,defaultModel)
        };
        if (EnableWebSocket)
            list.Add(GetWebSocketConfigGroup(isReadOnly, defaultModel));
        if (EnablePipeline)
            list.Add(GetPipelineConfigGroup(isReadOnly, defaultModel));
        if (EnableTcp)
            list.Add(GetTcpConfigGroup(isReadOnly, defaultModel));
        if (EnableHttp)
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
