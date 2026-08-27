# Quick.Protocol.InterfaceService

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.InterfaceService.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.InterfaceService/)

Quick.Protocol 的接口服务聚合层，提供统一的服务接口管理。

## 功能特性

- 统一管理多种传输方式（TCP/Pipeline/WebSocket/HTTP）
- 支持接口服务配置和上下文管理
- 简化多协议服务的部署和管理

## 安装

```bash
dotnet add package Quick.Protocol.InterfaceService
```

## 快速开始

```csharp
using Quick.Protocol.InterfaceService;

var config = new QpInterfaceServiceConfig
{
    TcpInterface = new TcpInterface { Port = 3000 },
    PipelineInterface = new PipeInterface { PipeName = "Quick.Protocol" },
    WebSocketInterface = new WebSocketInterface { Port = 3001 },
    HttpInterface = new HttpInterface { Port = 3002 },
    Password = "HelloQP"
};

var context = new QpInterfaceServiceContext(config);
context.Start();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
