# Quick.Protocol.WebSocket.Server.AspNetCore

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.WebSocket.Server.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.WebSocket.Server.AspNetCore/)

Quick.Protocol 的 WebSocket 服务端实现（基于 ASP.NET Core）。

## 功能特性

- 基于 ASP.NET Core 的 WebSocket 服务端
- 支持中间件集成
- 可与现有 ASP.NET Core 应用共存

## 安装

```bash
dotnet add package Quick.Protocol.WebSocket.Server.AspNetCore
```

## 快速开始

```csharp
using Quick.Protocol.WebSocket.Server.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseQuickProtocolWebSocketServer(new QpWebSocketServerOptions
{
    Password = "HelloQP"
});

app.Run();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
