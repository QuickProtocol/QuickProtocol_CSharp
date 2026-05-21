using System;
using Avalonia;

namespace QpTestClient;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Quick.Protocol.Pipeline.QpPipelineClientOptions.RegisterUriSchema();
        Quick.Protocol.Tcp.QpTcpClientOptions.RegisterUriSchema();
        Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();
        Quick.Protocol.Http.Client.QpHttpClientOptions.RegisterUriSchema();
        Quick.Protocol.SerialPort.QpSerialPortClientOptions.RegisterUriSchema();

        QpClientTypeManager.Instance.Init();

        var appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
        if (OperatingSystem.IsLinux())
        {
            appBuilder = appBuilder.WithFont_SourceHanSansCN();
        }
        return appBuilder;
    }
}
