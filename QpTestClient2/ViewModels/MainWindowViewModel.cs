using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using QpTestClient.Controls;
using QpTestClient.Utils;
using Quick.Protocol;
using Quick.Utils;
using System.Collections.ObjectModel;
using System.Reflection;
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

        public MainWindowViewModel(Window window)
        {
            this.window = window;
            Assembly assembly = GetType().Assembly;
            Title = $"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";

            ImportCommand = new DelegateCommand() { ExecuteCommand = executeImportCommand };

            var connectionInfos = QpdFileUtils.GetConnectionInfosFromQpbFileFolder();
            if (connectionInfos != null)
            {
                foreach (var connectionInfo in connectionInfos)
                    addConnection(connectionInfo);
            }
        }

        private void displayInstructions(TreeViewItem connectionNode, QpInstruction[] instructions)
        {
            connectionNode.Items.Clear();
            foreach (var instruction in instructions)
            {
                var instructionNode = new TreeViewItem()
                {
                     Name = instruction.Name
                };
                connectionNode.Items.Add(instructionNode);
                instructionNode.Tag = instruction;
                if (instruction.NoticeInfos != null)
                {
                    var noticesNode = new TreeViewItem()
                    {
                        Name = "通知"
                    };
                    instructionNode.Items.Add(noticesNode);
                    noticesNode.Tag = instruction.NoticeInfos;
                    foreach (var noticeInfo in instruction.NoticeInfos)
                    {
                        var noticeNode = new TreeViewItem()
                        {
                            Name = noticeInfo.Name
                        }; 
                        noticesNode.Items.Add(noticeNode);
                        //noticeNode.ContextMenuStrip = cmsNotice;
                        noticeNode.Tag = noticeInfo;
                    }
                }
                if (instruction.CommandInfos != null)
                {
                    var commandsNode = new TreeViewItem()
                    {
                        Name = "命令"
                    }; 
                    instructionNode.Items.Add(commandsNode);
                    commandsNode.Tag = instruction.CommandInfos;
                    foreach (var commandInfo in instruction.CommandInfos)
                    {
                        var commandNode = new TreeViewItem()
                        {
                            Name = commandInfo.Name
                        }; 
                        commandsNode.Items.Add(commandNode);
                        //commandNode.ContextMenuStrip = cmsCommand;
                        commandNode.Tag = commandInfo;
                    }
                }
            }
        }

        private void addConnection(TestConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                return;
            treeNodeCollection.Add(new ConnectionContext(connectionInfo));
            //connectionNode.ContextMenuStrip = cmsConnection;
            //if (connectionInfo.Instructions != null)
            //    displayInstructions(connectionNode, connectionInfo.Instructions);
        }

        private async void executeImportCommand(object e)
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
                    TestConnectionInfo connectionInfo = QpdFileUtils.Load(file.Path.LocalPath);
                    connectionInfo.Name = Path.GetFileNameWithoutExtension(file.Name);
                    addConnection(connectionInfo);
                    QpdFileUtils.SaveQpbFile(connectionInfo);
                }
                await MessageBoxManager.GetMessageBoxStandard(
                    Application.Current.Name,
                    "导入成功！",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info)
                    .ShowAsPopupAsync(window);
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(Application.Current.Name, $"导入失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                    .ShowWindowDialogAsync(window);
            }
        }
    }
}
