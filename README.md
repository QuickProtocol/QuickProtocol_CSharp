# QuickProtocol C#

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

一个简单、高性能的 .NET 通信协议，支持多种传输层。

## 功能特性

- **多传输层支持** - TCP、命名管道、串口、WebSocket、HTTP
- **命令与通知模式** - 支持 Request/Response 命令和单向通知
- **安全加密** - 支持 DES/AES 对称加密
- **数据压缩** - 支持 GZip 压缩
- **心跳机制** - 自动保持连接活跃
- **AOT 兼容** - 支持 Native AOT 编译
- **跨平台** - 支持 Windows、Linux、macOS

## NuGet 包

| 包名 | 版本 | 说明 |
|------|------|------|
| [Quick.Protocol](https://www.nuget.org/packages/Quick.Protocol/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.svg?style=flat) | 核心协议库 |
| [Quick.Protocol.Tcp](https://www.nuget.org/packages/Quick.Protocol.Tcp/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.Tcp.svg?style=flat) | TCP 传输层 |
| [Quick.Protocol.Pipeline](https://www.nuget.org/packages/Quick.Protocol.Pipeline/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.Pipeline.svg?style=flat) | 命名管道传输层 |
| [Quick.Protocol.SerialPort](https://www.nuget.org/packages/Quick.Protocol.SerialPort/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.SerialPort.svg?style=flat) | 串口传输层 |
| [Quick.Protocol.WebSocket.Client](https://www.nuget.org/packages/Quick.Protocol.WebSocket.Client/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.WebSocket.Client.svg?style=flat) | WebSocket 客户端 |
| [Quick.Protocol.WebSocket.Server.AspNetCore](https://www.nuget.org/packages/Quick.Protocol.WebSocket.Server.AspNetCore/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.WebSocket.Server.AspNetCore.svg?style=flat) | WebSocket 服务端 (ASP.NET Core) |
| [Quick.Protocol.Http.Client](https://www.nuget.org/packages/Quick.Protocol.Http.Client/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.Http.Client.svg?style=flat) | HTTP 客户端 |
| [Quick.Protocol.Http.Server.AspNetCore](https://www.nuget.org/packages/Quick.Protocol.Http.Server.AspNetCore/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.Http.Server.AspNetCore.svg?style=flat) | HTTP 服务端 (ASP.NET Core) |
| [Quick.Protocol.InterfaceService](https://www.nuget.org/packages/Quick.Protocol.InterfaceService/) | ![NuGet](http://img.shields.io/nuget/v/Quick.Protocol.InterfaceService.svg?style=flat) | 接口服务聚合层 |

## 安装

```bash
# 核心库
dotnet add package Quick.Protocol

# 根据需要选择传输层
dotnet add package Quick.Protocol.Tcp
dotnet add package Quick.Protocol.Pipeline
dotnet add package Quick.Protocol.SerialPort
dotnet add package Quick.Protocol.WebSocket.Client
dotnet add package Quick.Protocol.Http.Client
```

## 快速开始

### TCP 客户端

```csharp
using Quick.Protocol.Tcp;

// 注册 URI 协议
QpTcpClientOptions.RegisterUriSchema();

// 通过 URI 创建客户端
var options = QpClientOptions.Parse(new Uri("qp.tcp://127.0.0.1:3000?Password=HelloQP"));
var client = options.CreateClient();

// 连接
await client.ConnectAsync();

// 发送命令
var response = await client.SendCommand("MyNamespace.MyRequest", JsonSerializer.Serialize(new { Data = "Hello" }));

// 发送通知
await client.SendNoticePackage("MyNamespace.MyNotice", JsonSerializer.Serialize(new { Message = "Hi" }));

// 断开连接
client.Disconnect();
```

### TCP 服务端

```csharp
using Quick.Protocol.Tcp;

var server = new QpTcpServer(new QpTcpServerOptions
{
    Address = "0.0.0.0",
    Port = 3000,
    Password = "HelloQP"
});

// 监听连接事件
server.ChannelConnected += (sender, channel) =>
{
    Console.WriteLine($"客户端已连接: {channel.ChannelName}");
    
    // 注册命令处理器
    channel.RegisterCommandExecuterManagers(new[]
    {
        new CommandExecuterManager()
    });
};

server.Start();
Console.WriteLine("服务端已在端口 3000 启动");
```

### 命令定义

```csharp
using Quick.Protocol;

// 定义请求
public class MyRequest : IQpCommandRequest<MyRequest, MyResponse>
{
    public string Data { get; set; }
}

// 定义响应
public class MyResponse
{
    public string Result { get; set; }
}

// 注册命令处理器
var manager = new CommandExecuterManager();
manager.Register<MyRequest, MyResponse>((channel, request) =>
{
    return new MyResponse { Result = $"已收到: {request.Data}" };
});
```

## 协议规范

### 包结构

所有数字采用大端字节序，命令编号为 16 字节 GUID。

```
心跳包: [4字节包长度] [1字节包类型=0]

通知包: [4字节包长度] [1字节包类型=1] [1字节类名长度] [类名] [JSON内容]

命令请求: [4字节包长度] [1字节包类型=2] [16字节命令编号] [1字节类名长度] [类名] [JSON内容]

命令响应: [4字节包长度] [1字节包类型=3] [16字节命令编号] [1字节返回码]
          成功: [1字节类名长度] [类名] [JSON内容]
          失败: [错误消息]
```

### 连接流程

```
客户端                          服务端
  |                                |
  |--- 连接请求 ----------------->|
  |<-- 连接响应 ------------------|
  |                                |
  |--- 认证请求 ----------------->|
  |<-- 认证响应 ------------------|
  |                                |
  |--- 握手请求 ----------------->|
  |<-- 握手响应 ------------------|
  |                                |
  |=== 连接建立，开始心跳 ===|
```

## 项目结构

```
QuickProtocol_CSharp/
├── Quick.Protocol/                          # 核心协议库
├── Quick.Protocol.Tcp/                      # TCP 传输层
├── Quick.Protocol.Pipeline/                 # 命名管道传输层
├── Quick.Protocol.SerialPort/               # 串口传输层
├── Quick.Protocol.WebSocket.Client/         # WebSocket 客户端
├── Quick.Protocol.WebSocket.Server.AspNetCore/ # WebSocket 服务端
├── Quick.Protocol.Http.Client/              # HTTP 客户端
├── Quick.Protocol.Http.Server.AspNetCore/   # HTTP 服务端
├── Quick.Protocol.InterfaceService/         # 接口服务聚合层
├── QpTestClient/                            # 测试客户端 (Avalonia UI)
├── TestProgram/                             # 测试程序
└── SmokeTest/                               # 冒烟测试
```

## 系统要求

- .NET 10.0 或更高版本
- 支持 Windows、Linux、macOS

## 许可证

本项目采用 [Apache License 2.0](LICENSE) 许可证。

## 相关链接

- **GitHub**: https://github.com/QuickProtocol/QuickProtocol_CSharp
- **NuGet**: https://www.nuget.org/packages/Quick.Protocol/
- **文档**: https://github.com/QuickProtocol/QuickProtocol_CSharp/wiki

## 贡献

欢迎提交 Issue 和 Pull Request！

## 作者

- **scbeta** - [GitHub](https://github.com/QuickProtocol)
