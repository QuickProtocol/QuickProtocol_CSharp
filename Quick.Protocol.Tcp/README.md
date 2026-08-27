# Quick.Protocol.Tcp

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.Tcp.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.Tcp/)

Quick.Protocol 的 TCP 传输层实现。

## 功能特性

- 基于 TCP 协议的可靠传输
- 支持客户端和服务端
- 支持本地端点绑定
- 连接超时控制

## 安装

```bash
dotnet add package Quick.Protocol.Tcp
```

## 快速开始

### 客户端

```csharp
using Quick.Protocol.Tcp;

var options = new QpTcpClientOptions
{
    Host = "127.0.0.1",
    Port = 3000,
    Password = "HelloQP"
};

var client = new QpTcpClient(options);
await client.ConnectAsync();
```

### 服务端

```csharp
using Quick.Protocol.Tcp;

var options = new QpTcpServerOptions
{
    Address = "0.0.0.0",
    Port = 3000,
    Password = "HelloQP"
};

var server = new QpTcpServer(options);
server.ChannelConnected += (sender, channel) =>
{
    // 处理新连接
};
server.Start();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
