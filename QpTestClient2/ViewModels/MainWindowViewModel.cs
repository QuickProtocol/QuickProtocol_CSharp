using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using QpTestClient.Utils;
using Quick.Utils;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Windows.Input;

namespace QpTestClient.ViewModels
{
    public class MainWindowViewModel : PropertyNotifyModel
    {
        public const string QPDFILE_FILTER = "Quick.Protocol连接文件(*.qpd)|*.qpd";

        public ObservableCollection<ConnectionContext> treeNodeCollection { get; set; } = new ObservableCollection<ConnectionContext>();
        private Window window;
        public string Title { get; set; }

        public ICommand ImportCommand { get; set; }
        public ICommand ExitCommand { get; set; }        
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand DeleteCommand { get; set; }        
        public ICommand RecvHeartbeatCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public MainWindowViewModel(Window window)
        {
            this.window = window;
            Assembly assembly = GetType().Assembly;
            Title = $"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";

            ImportCommand = new DelegateCommand() { ExecuteCommand = executeImportCommand };
            ExitCommand = new DelegateCommand() { ExecuteCommand = executeExitCommand };
            ConnectCommand = new DelegateCommand() { ExecuteCommand = executeConnectCommand };
            DisconnectCommand = new DelegateCommand() { ExecuteCommand = executeDisconnectCommand };
            DeleteCommand = new DelegateCommand() { ExecuteCommand = executeDeleteCommand };
            RecvHeartbeatCommand = new DelegateCommand() { ExecuteCommand = executeRecvHeartbeatCommand };
            ExportCommand = new DelegateCommand() { ExecuteCommand = executeExportCommand };

            var connectionInfos = QpdFileUtils.GetConnectionInfosFromQpbFileFolder();
            if (connectionInfos != null)
            {
                foreach (var connectionInfo in connectionInfos)
                    addConnection(connectionInfo);
            }
        }

        private void addConnection(TestConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                return;
            treeNodeCollection.Add(new ConnectionContext(connectionInfo));
        }

        private async void executeImportCommand(object obj)
        {
            try
            {
                var ret = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    AllowMultiple = true,
                    FileTypeFilter = [new FilePickerFileType("Quick.Protocol连接文件(*.qpd)") { Patterns = ["*.qpd"] }]
                });
                if (ret.Count == 0)
                    return;
                foreach (var file in ret)
                {
                    TestConnectionInfo connectionInfo = QpdFileUtils.Load(file.TryGetLocalPath());
                    connectionInfo.Name = Path.GetFileNameWithoutExtension(file.Name);
                    addConnection(connectionInfo);
                    QpdFileUtils.SaveQpbFile(connectionInfo);
                }
                await MessageBoxManager.GetMessageBoxStandard(
                    Application.Current.Name,
                    "导入成功！",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Success)
                    .ShowAsPopupAsync(window);
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(null, $"导入失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                    .ShowAsPopupAsync(window);
            }
        }

        private async void executeExitCommand(object obj)
        {
            Environment.Exit(0);            
        }

        private async void executeConnectCommand(object obj)
        {
            var connectionContext=  (ConnectionContext)obj;
            try
            {
                var preConnectionInfoContent = JsonSerializer.Serialize(connectionContext.ConnectionInfo, TestConnectionInfoSerializerContext.Default2.TestConnectionInfo);

                await connectionContext.Connect();
                connectionContext.Disconnected += (sender, e) =>
                {
                    connectionContext.Dispose();
                };
                var currentConnectionInfoContent = JsonSerializer.Serialize(connectionContext.ConnectionInfo, TestConnectionInfoSerializerContext.Default2.TestConnectionInfo);
                if (currentConnectionInfoContent != preConnectionInfoContent)
                    QpdFileUtils.SaveQpbFile(connectionContext.ConnectionInfo);
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(null, $"连接失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                    .ShowAsPopupAsync(window);
            }
        }
        
        private void executeDisconnectCommand(object obj)
        {
            var connectionContext = (ConnectionContext)obj;
            connectionContext.Dispose();
        }

        private async void executeDeleteCommand(object obj)
        {
            var connectionContext = (ConnectionContext)obj;
            var ret = await MessageBoxManager.GetMessageBoxStandard(null, $"确定要删除连接[{connectionContext.ConnectionInfo.Name}]?    ", MsBox.Avalonia.Enums.ButtonEnum.OkCancel, MsBox.Avalonia.Enums.Icon.Question)
                    .ShowAsPopupAsync(window);
            if (ret == MsBox.Avalonia.Enums.ButtonResult.Cancel)
                return;

            connectionContext.Dispose();
            treeNodeCollection.Remove(connectionContext);
            QpdFileUtils.DeleteQpbFile(connectionContext.ConnectionInfo);
        }

        private void executeRecvHeartbeatCommand(object obj)
        {
            var connectionContext = (ConnectionContext)obj;
            new HeartbeatRecvForm(connectionContext).Show();
        }

        private async void executeExportCommand(object obj)
        {
            var connectionContext = (ConnectionContext)obj;
            try
            {
                var ret = await window.StorageProvider.SaveFilePickerAsync(new()
                {
                    SuggestedFileName = $"{connectionContext.ConnectionInfo.Name}.qpd",
                    FileTypeChoices = [new FilePickerFileType("Quick.Protocol连接文件(*.qpd)") { Patterns = ["*.qpd"] }]
                });
                if (ret == null)
                    return;
                var file = ret.TryGetLocalPath();
                QpdFileUtils.SaveQpbFile(connectionContext.ConnectionInfo, file);
                await MessageBoxManager.GetMessageBoxStandard(null, $"导出成功！", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success)
                    .ShowAsPopupAsync(window);
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(null, $"导出失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                    .ShowAsPopupAsync(window);
            }
        }
    }
}
