using Quick.Protocol.Pipeline;
using Quick.Utils;

namespace Quick.Protocol.InterfaceService.Interfaces
{
    internal class PipeInterface
    {
        public const string INTERFACE_TYPE = "管道";
        private QpInterfaceServiceContextOptions interfaceOptions;
        private QpPipelineServerOptions options;
        private QpPipelineServer server;
        public QpChannel[] GetAllChannels() => server?.Channels ?? new QpChannel[0];

        public PipeInterface(QpInterfaceServiceContextOptions interfaceOptions)
        {
            this.interfaceOptions = interfaceOptions;
            options = new QpPipelineServerOptions()
            {
                PipeName = interfaceOptions.Config.PipeName,
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
            server = new QpPipelineServer(options);
            try
            {
                server.Start();
                interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：qp.pipe://./{interfaceOptions.Config.PipeName}");
            }
            catch (Exception ex)
            {
                interfaceOptions.Logger?.Invoke($"[{interfaceOptions.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：qp.pipe://./{interfaceOptions.Config.PipeName}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
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
