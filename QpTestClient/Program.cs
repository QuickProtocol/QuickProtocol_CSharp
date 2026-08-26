using Avalonia;
using System;

namespace QpTestClient
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Quick.Protocol.Pipeline.QpPipelineClientOptions.RegisterUriSchema();
            Quick.Protocol.Tcp.QpTcpClientOptions.RegisterUriSchema();
            Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();
            Quick.Protocol.Http.Client.QpHttpClientOptions.RegisterUriSchema();
            Quick.Protocol.SerialPort.QpSerialPortClientOptions.RegisterUriSchema();

            QpClientTypeManager.Instance.Init();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
