# Quick.Protocol.Http.Server.AspNetCore

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.Http.Server.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.Http.Server.AspNetCore/)

Quick.Protocol 的 HTTP 服务端实现（基于 ASP.NET Core）。

## 功能特性

- 基于 ASP.NET Core 的 HTTP 服务端
- 支持中间件集成
- 可与现有 ASP.NET Core 应用共存

## 安装

```bash
dotnet add package Quick.Protocol.Http.Server.AspNetCore
```

## 快速开始

```csharp
using Quick.Protocol.Http.Server.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseQuickProtocolHttpServer(new QpHttpServerOptions
{
    Password = "HelloQP"
});

app.Run();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
