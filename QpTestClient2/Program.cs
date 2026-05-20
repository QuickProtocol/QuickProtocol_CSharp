using System;
using Avalonia;

namespace QpTestClient2;

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
