# Quick.Protocol.Http.Client

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.Http.Client.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.Http.Client/)

Quick.Protocol 的 HTTP 客户端传输层实现。

## 功能特性

- 基于 HTTP 协议的网络通信
- 支持跨平台
- 适用于 Web API 和防火墙友好的场景

## 安装

```bash
dotnet add package Quick.Protocol.Http.Client
```

## 快速开始

```csharp
using Quick.Protocol.Http.Client;

var options = new QpHttpClientOptions
{
    Url = "http://127.0.0.1:3000",
    Password = "HelloQP"
};

var client = new QpHttpClient(options);
await client.ConnectAsync();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
