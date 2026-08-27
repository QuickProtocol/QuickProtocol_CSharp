# Quick.Protocol.WebSocket.Client

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.WebSocket.Client.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.WebSocket.Client/)

Quick.Protocol 的 WebSocket 客户端传输层实现。

## 功能特性

- 基于 WebSocket 协议的网络通信
- 支持跨平台
- 适用于 Web 应用和防火墙友好的场景

## 安装

```bash
dotnet add package Quick.Protocol.WebSocket.Client
```

## 快速开始

```csharp
using Quick.Protocol.WebSocket.Client;

var options = new QpWebSocketClientOptions
{
    Url = "ws://127.0.0.1:3000",
    Password = "HelloQP"
};

var client = new QpWebSocketClient(options);
await client.ConnectAsync();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
