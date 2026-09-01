# Quick.Protocol

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol/)

Quick.Protocol 核心库，提供协议基础实现。

> 核心库仅包含协议基础能力，不包含具体传输实现。使用 TCP/WebSocket/HTTP 等传输时，需额外安装对应的传输层包，例如 `Quick.Protocol.Tcp`（提供 `QpTcpClientOptions`/`QpTcpServerOptions` 等）。

## 功能特性

- 基于 TCP / SerialPort / WebSocket / HTTP / Pipeline / STDIO 的通信协议
- 支持命令（Request/Response）和通知（Notice）模式
- 支持 DES/AES 加密
- 支持 GZip 压缩
- 心跳机制保持连接活跃
- AOT 兼容（命令/通知类型需提供源生成的 `JsonSerializerContext` 序列化器）

## 安装

```bash
dotnet add package Quick.Protocol
```

## 快速开始

```csharp
using Quick.Protocol;
using Quick.Protocol.Commands.PrivateCommand;
using Quick.Protocol.Notices;

// 创建客户端选项
var options = new QpTcpClientOptions
{
    Host = "127.0.0.1",
    Port = 3000,
    Password = "HelloQP"
};

// 创建并连接客户端（需先启动监听 127.0.0.1:3000 的服务端）
var client = options.CreateClient();
await client.ConnectAsync();

// 发送命令（PrivateCommand 为内置命令，服务端需注册对应命令执行器）
var response = await client.SendCommand(new Request { Content = "ping" });

// 发送通知（服务端需注册对应通知处理器）
await client.SendNoticePackage(new PrivateNotice { Action = "greet", Content = "hello" });
```

> 说明：示例使用了库内置的 `PrivateCommand` / `PrivateNotice` 类型，可直接编译运行；
> 实际项目中请定义自己的命令/通知类型（继承 `AbstractQpSerializer<T>` 并提供源生成 `JsonSerializerContext`），
> 并在服务端通过 `RegisterCommandExecuterManager` / `RegisterNoticeHandlerManager` 注册对应处理器。

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
