using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using QpTestClient.Controls;
using QpTestClient.Forms;
using QpTestClient.Utils;
using Quick.Protocol;
using Quick.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QpTestClient
{
    public partial class MainWindow : Window
    {
        public const string QPDFILE_FILTER = "qpd";

        private TreeViewItem? selectedConnectionNode;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            var connectionInfos = QpdFileUtils.GetConnectionInfosFromQpbFileFolder();
            if (connectionInfos != null)
            {
                foreach (var connectionInfo in connectionInfos)
                    AddConnection(connectionInfo);
            }
        }

        private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            var items = tvQpInstructions.Items.Cast<TreeViewItem>().ToList();
            foreach (var item in items)
            {
                if (item.Tag is ConnectionContext connectionContext)
                    connectionContext.Dispose();
            }
        }

        private async void BtnImportConnectionFile_Click(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "导入连接文件",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Quick.Protocol连接文件") { Patterns = new[] { "*.qpd" } } }
            });

            if (files.Count == 0)
                return;

            try
            {
                var file = files[0].Path.LocalPath;
                TestConnectionInfo connectionInfo = QpdFileUtils.Load(file);
                connectionInfo.Name = Path.GetFileNameWithoutExtension(file);
                AddConnection(connectionInfo);
                QpdFileUtils.SaveQpbFile(connectionInfo);
                await MessageBox.Show(this, "导入成功！", "提示");
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"导入失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", "错误");
            }
        }

        private void BtnExit_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowContent(Control? item)
        {
            ContentPresenter.Content = item;
        }

        private void TvQpInstructions_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;

            var nodeObj = node.Tag;
            GbNodeInfoHeader.Text = node.Header?.ToString() ?? "请选择节点";

            if (nodeObj == null)
            {
                ShowContent(null);
            }
            else if (nodeObj is ConnectionContext connectionContext)
            {
                ShowContent(new ConnectionInfoControl(connectionContext));
            }
            else if (nodeObj is QpInstruction qpInstruction)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"指令集编号：{qpInstruction.Id}");
                sb.AppendLine($"指令集名称：{qpInstruction.Name}");
                if (qpInstruction.CommandInfos != null && qpInstruction.CommandInfos.Length > 0)
                {
                    sb.AppendLine($"命令：");
                    foreach (var cmdInfo in qpInstruction.CommandInfos)
                        sb.AppendLine($"    {cmdInfo.Name}");
                }
                if (qpInstruction.NoticeInfos != null && qpInstruction.NoticeInfos.Length > 0)
                {
                    sb.AppendLine($"通知：");
                    foreach (var noticeInfo in qpInstruction.NoticeInfos)
                        sb.AppendLine($"    {noticeInfo.Name}");
                }
                ShowContent(new TextBox { Text = sb.ToString(), IsReadOnly = true, AcceptsReturn = true });
            }
            else if (nodeObj is QpNoticeInfo[] noticeInfos)
            {
                var sb = new StringBuilder();
                if (noticeInfos != null && noticeInfos.Length > 0)
                {
                    foreach (var noticeInfo in noticeInfos)
                    {
                        sb.AppendLine($"通知名称：{noticeInfo.Name}");
                        sb.AppendLine($"类名称：{noticeInfo.NoticeTypeName}");
                        sb.AppendLine();
                    }
                }
                ShowContent(new TextBox { Text = sb.ToString(), IsReadOnly = true, AcceptsReturn = true });
            }
            else if (nodeObj is QpNoticeInfo noticeInfo)
            {
                ShowContent(new NoticeInfoControl(noticeInfo));
            }
            else if (nodeObj is QpCommandInfo[] cmdInfos)
            {
                var sb = new StringBuilder();
                if (cmdInfos != null && cmdInfos.Length > 0)
                {
                    foreach (var cmdInfo in cmdInfos)
                    {
                        sb.AppendLine($"命令名称：{cmdInfo.Name}");
                        sb.AppendLine($"请求类名称：{cmdInfo.RequestTypeName}");
                        sb.AppendLine($"响应类名称：{cmdInfo.ResponseTypeName}");
                        sb.AppendLine();
                    }
                }
                ShowContent(new TextBox { Text = sb.ToString(), IsReadOnly = true, AcceptsReturn = true });
            }
            else if (nodeObj is QpCommandInfo commandInfo)
            {
                ShowContent(new CommandInfoControl(commandInfo));
            }
        }

        private async void TvQpInstructions_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;

            var nodeObj = node.Tag;
            if (nodeObj == null)
                return;

            if (nodeObj is ConnectionContext connectionContext)
            {
                if (!connectionContext.Connected)
                    await ConnectConnectionAsync(node, connectionContext);
            }
            else if (nodeObj is QpNoticeInfo)
            {
                OpenNoticeRecvForm(node);
            }
            else if (nodeObj is QpCommandInfo)
            {
                OpenCommandTestForm(node);
            }
        }

        private ConnectionContext? FindConnectionContext(TreeViewItem item)
        {
            var current = item;
            while (current != null)
            {
                if (current.Tag is ConnectionContext ctx)
                    return ctx;
                current = current.Parent as TreeViewItem;
            }
            return null;
        }

        private void CmsConnection_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
            {
                e.Cancel = true;
                return;
            }

            var connectionContext = FindConnectionContext(node);
            if (connectionContext == null)
            {
                e.Cancel = true;
                return;
            }

            UpdateContextMenuVisibility(connectionContext);
        }

        private void UpdateContextMenuVisibility(ConnectionContext connectionContext)
        {
            if (connectionContext.Connected)
            {
                btnConnectConnection.IsVisible = false;
                btnDisconnectConnection.IsVisible = true;
                separatorConnection.IsVisible = true;
                btnRecvHeartbeat_Connection.IsVisible = true;
                btnRecvNotice_Connection.IsVisible = true;
                btnTestCommand_Connection.IsVisible = true;
                btnEditConnection.IsVisible = false;
                btnDelConnection.IsVisible = false;
                btnExportConnectionFile.IsVisible = false;
                btnGenerateConnectionUrl.IsVisible = false;
            }
            else
            {
                btnConnectConnection.IsVisible = true;
                btnDisconnectConnection.IsVisible = false;
                separatorConnection.IsVisible = false;
                btnRecvHeartbeat_Connection.IsVisible = false;
                btnRecvNotice_Connection.IsVisible = false;
                btnTestCommand_Connection.IsVisible = false;
                btnEditConnection.IsVisible = true;
                btnDelConnection.IsVisible = true;
                btnExportConnectionFile.IsVisible = true;
                btnGenerateConnectionUrl.IsVisible = true;
            }
        }

        private void DisplayInstructions(TreeViewItem connectionNode, QpInstruction[] instructions)
        {
            connectionNode.Items.Clear();
            foreach (var instruction in instructions)
            {
                var instructionNode = new TreeViewItem
                {
                    Header = instruction.Name,
                    Tag = instruction
                };
                connectionNode.Items.Add(instructionNode);

                if (instruction.NoticeInfos != null)
                {
                    var noticesNode = new TreeViewItem
                    {
                        Header = "通知",
                        Tag = instruction.NoticeInfos
                    };
                    instructionNode.Items.Add(noticesNode);
                    foreach (var noticeInfo in instruction.NoticeInfos)
                    {
                        var noticeNode = new TreeViewItem
                        {
                            Header = noticeInfo.Name,
                            Tag = noticeInfo
                        };
                        noticesNode.Items.Add(noticeNode);
                    }
                }
                if (instruction.CommandInfos != null)
                {
                    var commandsNode = new TreeViewItem
                    {
                        Header = "命令",
                        Tag = instruction.CommandInfos
                    };
                    instructionNode.Items.Add(commandsNode);
                    foreach (var commandInfo in instruction.CommandInfos)
                    {
                        var commandNode = new TreeViewItem
                        {
                            Header = commandInfo.Name,
                            Tag = commandInfo
                        };
                        commandsNode.Items.Add(commandNode);
                    }
                }
            }
        }

        private void AddConnection(TestConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                return;
            var connectionNode = new TreeViewItem
            {
                Header = connectionInfo.Name,
                Tag = new ConnectionContext(connectionInfo)
            };
            tvQpInstructions.Items.Add(connectionNode);
            if (connectionInfo.Instructions != null)
                DisplayInstructions(connectionNode, connectionInfo.Instructions);
        }

        private async void BtnQuickAddConnection_Click(object? sender, RoutedEventArgs e)
        {
            var form = new QuickConnectForm();
            var result = await form.ShowDialog<TestConnectionInfo?>(this);
            if (result == null)
                return;
            AddConnection(result);
            QpdFileUtils.SaveQpbFile(result);
        }

        private async void BtnAddConnection_Click(object? sender, RoutedEventArgs e)
        {
            var form = new ConnectForm();
            var result = await form.ShowDialog<TestConnectionInfo?>(this);
            if (result == null)
                return;
            AddConnection(result);
            QpdFileUtils.SaveQpbFile(result);
        }

        private void BtnDisconnectConnection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is ConnectionContext connectionContext)
                connectionContext.Dispose();
        }

        private async void BtnDelConnection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;

            var result = await MessageBox.Show(this, $"确定要删除连接[{connectionContext.ConnectionInfo.Name}]?", "删除确认", MessageBoxButtons.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            connectionContext.Dispose();
            tvQpInstructions.Items.Remove(node);
            QpdFileUtils.DeleteQpbFile(connectionContext.ConnectionInfo);
        }

        private async Task ConnectConnectionAsync(TreeViewItem connectionNode, ConnectionContext connectionContext)
        {
            this.IsEnabled = false;
            try
            {
                var preConnectionInfoContent = JsonSerializer.Serialize(connectionContext.ConnectionInfo, TestConnectionInfoSerializerContext.Default2.TestConnectionInfo);

                await connectionContext.Connect();
                connectionContext.Disconnected += (s, e) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        connectionContext.Dispose();
                    });
                };
                DisplayInstructions(connectionNode, connectionContext.ConnectionInfo.Instructions);
                connectionNode.IsExpanded = true;

                var currentConnectionInfoContent = JsonSerializer.Serialize(connectionContext.ConnectionInfo, TestConnectionInfoSerializerContext.Default2.TestConnectionInfo);
                if (currentConnectionInfoContent != preConnectionInfoContent)
                    QpdFileUtils.SaveQpbFile(connectionContext.ConnectionInfo);
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"连接失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", "错误");
            }
            this.IsEnabled = true;
        }

        private async void BtnConnectConnection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            await ConnectConnectionAsync(node, connectionContext);
        }

        private void BtnRecvHeartbeat_Connection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new HeartbeatRecvForm(connectionContext);
            form.Show(this);
        }

        private void BtnRecvNotice_Connection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new NoticeRecvForm(connectionContext);
            form.Show(this);
        }

        private void BtnTestCommand_Connection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new CommandTestForm(connectionContext);
            form.Show(this);
        }

        private async void BtnEditConnection_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;

            var form = new ConnectForm();
            form.EditConnectionInfo(connectionContext.ConnectionInfo);
            var result = await form.ShowDialog<TestConnectionInfo?>(this);
            if (result == null)
                return;

            connectionContext.Dispose();
            tvQpInstructions.Items.Remove(node);
            AddConnection(result);
            QpdFileUtils.SaveQpbFile(result);
        }

        private async void BtnExportConnectionFile_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;

            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "导出连接文件",
                FileTypeChoices = new[] { new FilePickerFileType("Quick.Protocol连接文件") { Patterns = new[] { "*.qpd" } } },
                SuggestedFileName = connectionContext.ConnectionInfo.Name + ".qpd"
            });

            if (file == null)
                return;

            QpdFileUtils.SaveQpbFile(connectionContext.ConnectionInfo, file.Path.LocalPath);
            await MessageBox.Show(this, "导出成功！", "提示");
        }

        private async void BtnGenerateConnectionUrl_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new GenerateConnectionUrl(connectionContext);
            await form.ShowDialog(this);
        }

        private void OpenNoticeRecvForm(TreeViewItem noticeNode)
        {
            if (noticeNode.Tag is not QpNoticeInfo qpNoticeInfo)
                return;
            var connectionContext = FindConnectionContext(noticeNode);
            if (connectionContext == null)
                return;
            var form = new NoticeRecvForm(connectionContext, qpNoticeInfo);
            form.Show(this);
        }

        private void OpenCommandTestForm(TreeViewItem commandNode)
        {
            if (commandNode.Tag is not QpCommandInfo qpCommandInfo)
                return;
            var connectionContext = FindConnectionContext(commandNode);
            if (connectionContext == null)
                return;
            var form = new CommandTestForm(connectionContext, qpCommandInfo);
            form.Show(this);
        }

        private void BtnRecvNotice_Notice_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            OpenNoticeRecvForm(node);
        }

        private void BtnTestCommand_Command_Click(object? sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            OpenCommandTestForm(node);
        }

        private async void AboutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var form = new AboutBox();
            await form.ShowDialog(this);
        }
    }
}
