using Quick.Protocol.Tcp;
using Quick.Utils;

namespace Quick.Protocol.InterfaceService.Interfaces
{
    internal class TcpInterface
    {
        public const string INTERFACE_TYPE = "TCP";
        private QpInterfaceServiceContextOptions interfaceOptions;
        private QpTcpServerOptions options;
        private QpTcpServer server;
        public QpChannel[] GetAllChannels() => server?.Channels ?? new QpChannel[0];

        public TcpInterface(QpInterfaceServiceContextOptions interfaceOptions)
        {
            this.interfaceOptions = interfaceOptions;
            options = new QpTcpServerOptions()
            {
                Address = System.Net.IPAddress.Parse(interfaceOptions.Config.TcpListenAddress),
                Port = interfaceOptions.Config.TcpListenPort,
                Password = interfaceOptions.Config.Password,
                InstructionSet = interfaceOptions.InstructionSet,
                MaxPackageSize = interfaceOptions.Config.MaxPackageSize
            };
            if (interfaceOptions.CommandExecuterManager != null)
                options.RegisterCommandExecuterManager(interfaceOptions.CommandExecuterManager);
            if (interfaceOptions.NoticeHandlerManager != null)
                options.RegisterNoticeHandlerManager(interfaceOptions.NoticeHandlerManager);
        }

        public void Start()
        {
            server = new QpTcpServer(options);
            try
            {
                server.Start();
                interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：qp.tcp://{interfaceOptions.Config.TcpListenAddress}:{interfaceOptions.Config.TcpListenPort}");
            }
            catch (Exception ex)
            {
                interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：qp.tcp://{interfaceOptions.Config.TcpListenAddress}:{interfaceOptions.Config.TcpListenPort}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
                Stop();
                return;
            }
        }

        public void Stop()
        {
            server?.Stop();
            server = null;
            interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已停止");
        }
    }
}
