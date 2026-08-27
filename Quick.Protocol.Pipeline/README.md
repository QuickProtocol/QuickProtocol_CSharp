# Quick.Protocol.Pipeline

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.Pipeline.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.Pipeline/)

Quick.Protocol 的命名管道传输层实现。

## 功能特性

- 基于命名管道的本地进程间通信
- 支持客户端和服务端
- 跨平台支持（Windows/Linux/macOS）

## 安装

```bash
dotnet add package Quick.Protocol.Pipeline
```

## 快速开始

### 客户端

```csharp
using Quick.Protocol.Pipeline;

var options = new QpPipelineClientOptions
{
    ServerName = ".",
    PipeName = "Quick.Protocol",
    Password = "HelloQP"
};

var client = new QpPipelineClient(options);
await client.ConnectAsync();
```

### 服务端

```csharp
using Quick.Protocol.Pipeline;

var options = new QpPipelineServerOptions
{
    PipeName = "Quick.Protocol",
    Password = "HelloQP"
};

var server = new QpPipelineServer(options);
server.Start();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
