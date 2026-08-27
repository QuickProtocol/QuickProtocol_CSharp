# Quick.Protocol

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol/)

Quick.Protocol 核心库，提供协议基础实现。

## 功能特性

- 基于 TCP/Pipeline/SerialPort/WebSocket/HTTP 的通信协议
- 支持命令（Request/Response）和通知（Notice）模式
- 支持 DES/AES 加密
- 支持 GZip 压缩
- 心跳机制保持连接活跃
- AOT 兼容

## 安装

```bash
dotnet add package Quick.Protocol
```

## 快速开始

```csharp
using Quick.Protocol;

// 创建客户端选项
var options = new QpTcpClientOptions
{
    Host = "127.0.0.1",
    Port = 3000,
    Password = "HelloQP"
};

// 创建并连接客户端
var client = options.CreateClient();
await client.ConnectAsync();

// 发送命令
var response = await client.SendCommand(new MyRequest { ... });

// 发送通知
await client.SendNoticePackage(new MyNotice { ... });
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
