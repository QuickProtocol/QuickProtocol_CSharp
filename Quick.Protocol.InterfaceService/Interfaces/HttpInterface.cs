using Quick.Utils;

namespace Quick.Protocol.InterfaceService.Interfaces;

internal class HttpInterface
{
    public const string INTERFACE_TYPE = "HTTP";
    private QpInterfaceServiceContextOptions interfaceOptions;
    public QpChannel[] GetAllChannels() => interfaceOptions.HttpServer?.Channels ?? new QpChannel[0];

    public HttpInterface(QpInterfaceServiceContextOptions interfaceOptions)
    {
        this.interfaceOptions = interfaceOptions;
        interfaceOptions.HttpServerOptions.Password = interfaceOptions.Config.HttpServerOptions.Password;
        interfaceOptions.HttpServerOptions.MaxPackageSize = interfaceOptions.Config.HttpServerOptions.MaxPackageSize;
        if (interfaceOptions.CommandExecuterManager != null && !interfaceOptions.HttpServerOptions.CommandExecuterManagerList.Contains(interfaceOptions.CommandExecuterManager))
            interfaceOptions.HttpServerOptions.RegisterCommandExecuterManager(interfaceOptions.CommandExecuterManager);
        if (interfaceOptions.NoticeHandlerManager != null && !interfaceOptions.HttpServerOptions.NoticeHandlerManagerList.Contains(interfaceOptions.NoticeHandlerManager))
            interfaceOptions.HttpServerOptions.RegisterNoticeHandlerManager(interfaceOptions.NoticeHandlerManager);
    }

    public void Start()
    {
        var httpUrl = $"qp.http://xxx.xxx.xxx.xxx:xxx{interfaceOptions.Config.HttpServerOptions.Path}";
        try
        {
            interfaceOptions.HttpServer.Start();
            interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：{httpUrl}");
        }
        catch (Exception ex)
        {
            interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：{httpUrl}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
            Stop();
            return;
        }
    }

    public void Stop()
    {
        interfaceOptions.HttpServer.Stop();
        interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已停止");
    }
}
