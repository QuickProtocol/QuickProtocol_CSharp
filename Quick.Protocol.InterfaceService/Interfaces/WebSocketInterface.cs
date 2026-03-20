using Quick.Utils;

namespace Quick.Protocol.InterfaceService.Interfaces
{
    internal class WebSocketInterface
    {
        public const string INTERFACE_TYPE = "WebSocket";
        private QpInterfaceServiceContextOptions interfaceOptions;
        public QpChannel[] GetAllChannels() => interfaceOptions.WebSocketServer?.Channels ?? new QpChannel[0];

        public WebSocketInterface(QpInterfaceServiceContextOptions interfaceOptions)
        {
            this.interfaceOptions = interfaceOptions;
            interfaceOptions.WebSocketServerOptions.Password = interfaceOptions.Config.WebSocketServerOptions.Password;
            interfaceOptions.WebSocketServerOptions.MaxPackageSize = interfaceOptions.Config.WebSocketServerOptions.MaxPackageSize;
            if (interfaceOptions.CommandExecuterManager != null && !interfaceOptions.WebSocketServerOptions.CommandExecuterManagerList.Contains(interfaceOptions.CommandExecuterManager))
                interfaceOptions.WebSocketServerOptions.RegisterCommandExecuterManager(interfaceOptions.CommandExecuterManager);
            if (interfaceOptions.NoticeHandlerManager != null && !interfaceOptions.WebSocketServerOptions.NoticeHandlerManagerList.Contains(interfaceOptions.NoticeHandlerManager))
                interfaceOptions.WebSocketServerOptions.RegisterNoticeHandlerManager(interfaceOptions.NoticeHandlerManager);
        }

        public void Start()
        {
            var wsUrl = $"qp.ws://xxx.xxx.xxx.xxx:xxx{interfaceOptions.Config.WebSocketServerOptions.Path}";
            try
            {
                interfaceOptions.WebSocketServer.Start();
                interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：{wsUrl}");
            }
            catch (Exception ex)
            {
                interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：{wsUrl}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
                Stop();
                return;
            }
        }

        public void Stop()
        {
            interfaceOptions.WebSocketServer.Stop();
            interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已停止");
        }
    }
}
