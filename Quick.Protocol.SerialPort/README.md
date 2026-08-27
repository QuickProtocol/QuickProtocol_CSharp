# Quick.Protocol.SerialPort

[![NuGet Version](http://img.shields.io/nuget/v/Quick.Protocol.SerialPort.svg?style=flat)](https://www.nuget.org/packages/Quick.Protocol.SerialPort/)

Quick.Protocol 的串口传输层实现。

## 功能特性

- 基于串口的设备通信
- 支持自定义波特率、数据位、停止位、校验位
- 适用于嵌入式设备和工业控制场景

## 安装

```bash
dotnet add package Quick.Protocol.SerialPort
```

## 快速开始

### 客户端

```csharp
using Quick.Protocol.SerialPort;

var options = new QpSerialPortClientOptions
{
    PortName = "COM1",
    BaudRate = 9600,
    Password = "HelloQP"
};

var client = new QpSerialPortClient(options);
await client.ConnectAsync();
```

## 项目地址

https://github.com/QuickProtocol/QuickProtocol_CSharp
