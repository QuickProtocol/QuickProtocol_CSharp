using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using QpTestClient.Controls;
using QpTestClient.Forms;
using QpTestClient.Utils;
using Quick.Protocol;
using Quick.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;

namespace QpTestClient
{
    public partial class MainWindow : Window
    {
        public const string QPDFILE_FILTER = "qpd";

        private ContextMenu _cmsConnection;
        private ContextMenu _cmsNotice;
        private ContextMenu _cmsCommand;

        // 连接菜单项引用
        private MenuItem _btnDisconnectConnection;
        private MenuItem _btnConnectConnection;
        private Separator _separatorConnection;
        private MenuItem _btnRecvHeartbeat_Connection;
        private MenuItem _btnRecvNotice_Connection;
        private MenuItem _btnTestCommand_Connection;
        private MenuItem _btnEditConnection;
        private MenuItem _btnDelConnection;
        private MenuItem _btnExportConnectionFile;
        private MenuItem _btnGenerateConnectionUrl;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += $" ver:{ProductInfoUtils.GetAssemblyVersion()}";

            // 创建连接节点右键菜单
            _btnDisconnectConnection = new MenuItem { Header = "断开(_D)", IsVisible = false };
            _btnDisconnectConnection.Click += BtnDisconnectConnection_Click;

            _btnConnectConnection = new MenuItem { Header = "连接" };
            _btnConnectConnection.Click += BtnConnectConnection_Click;

            _separatorConnection = new Separator { IsVisible = false };

            _btnRecvHeartbeat_Connection = new MenuItem { Header = "接收心跳(_H)..", IsVisible = false };
            _btnRecvHeartbeat_Connection.Click += BtnRecvHeartbeat_Connection_Click;

            _btnRecvNotice_Connection = new MenuItem { Header = "接收通知(_R)..", IsVisible = false };
            _btnRecvNotice_Connection.Click += BtnRecvNotice_Connection_Click;

            _btnTestCommand_Connection = new MenuItem { Header = "测试命令(_T)..", IsVisible = false };
            _btnTestCommand_Connection.Click += BtnTestCommand_Connection_Click;

            var separatorEdit = new Separator();

            _btnEditConnection = new MenuItem { Header = "编辑(_E).." };
            _btnEditConnection.Click += BtnEditConnection_Click;

            _btnDelConnection = new MenuItem { Header = "删除(_D)" };
            _btnDelConnection.Click += BtnDelConnection_Click;

            _btnExportConnectionFile = new MenuItem { Header = "导出(_X).." };
            _btnExportConnectionFile.Click += BtnExportConnectionFile_Click;

            _btnGenerateConnectionUrl = new MenuItem { Header = "生成URL(_U).." };
            _btnGenerateConnectionUrl.Click += BtnGenerateConnectionUrl_Click;

            _cmsConnection = new ContextMenu
            {
                Items =
                {
                    _btnDisconnectConnection,
                    _btnConnectConnection,
                    _separatorConnection,
                    _btnRecvHeartbeat_Connection,
                    _btnRecvNotice_Connection,
                    _btnTestCommand_Connection,
                    separatorEdit,
                    _btnEditConnection,
                    _btnDelConnection,
                    _btnExportConnectionFile,
                    _btnGenerateConnectionUrl
                }
            };
            _cmsConnection.Opening += CmsConnection_Opening;

            // 创建通知节点右键菜单
            var btnRecvNotice = new MenuItem { Header = "接收通知(_R).." };
            btnRecvNotice.Click += BtnRecvNotice_Notice_Click;
            _cmsNotice = new ContextMenu { Items = { btnRecvNotice } };

            // 创建命令节点右键菜单
            var btnTestCommand = new MenuItem { Header = "测试(_T).." };
            btnTestCommand.Click += BtnTestCommand_Command_Click;
            _cmsCommand = new ContextMenu { Items = { btnTestCommand } };

            // 加载连接信息
            var connectionInfos = QpdFileUtils.GetConnectionInfosFromQpbFileFolder();
            if (connectionInfos != null)
            {
                foreach (var connectionInfo in connectionInfos)
                    AddConnection(connectionInfo);
            }
        }

        private async void MainWindow_Closing(object sender, WindowClosingEventArgs e)
        {
            var items = tvQpInstructions.Items.Cast<TreeViewItem>().ToList();
            foreach (var item in items)
            {
                if (item.Tag is ConnectionContext connectionContext)
                    connectionContext.Dispose();
            }
        }

        private async void BtnImportConnectionFile_Click(object sender, RoutedEventArgs e)
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

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowContent(Control item)
        {
            ContentPresenter.Content = item;
        }

        private void TvQpInstructions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(tvQpInstructions.SelectedItem==null)
            {
                GbNodeInfoHeader.Text = "请选择节点";
                ShowContent(null);
                return;
            }
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;

            var nodeObj = node.Tag;

            GbNodeInfoHeader.Text = ((Control)node.Header).Tag.ToString();

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
                ShowContent(new TextBox { Text = sb.ToString(), IsReadOnly = true, VerticalAlignment = VerticalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Top });
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
                ShowContent(new TextBox { Text = sb.ToString(), IsReadOnly = true, VerticalAlignment = VerticalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Top });
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

        private async void TvQpInstructions_DoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
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

        private ConnectionContext FindConnectionContext(TreeViewItem item)
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

        private void CmsConnection_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
            {
                e.Cancel = true;
                return;
            }

            // 只在连接节点上显示连接菜单
            if (node.Tag is not ConnectionContext connectionContext)
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
                _btnConnectConnection.IsVisible = false;
                _btnDisconnectConnection.IsVisible = true;
                _separatorConnection.IsVisible = true;
                _btnRecvHeartbeat_Connection.IsVisible = true;
                _btnRecvNotice_Connection.IsVisible = true;
                _btnTestCommand_Connection.IsVisible = true;
                _btnEditConnection.IsVisible = false;
                _btnDelConnection.IsVisible = false;
                _btnExportConnectionFile.IsVisible = false;
                _btnGenerateConnectionUrl.IsVisible = false;
            }
            else
            {
                _btnConnectConnection.IsVisible = true;
                _btnDisconnectConnection.IsVisible = false;
                _separatorConnection.IsVisible = false;
                _btnRecvHeartbeat_Connection.IsVisible = false;
                _btnRecvNotice_Connection.IsVisible = false;
                _btnTestCommand_Connection.IsVisible = false;
                _btnEditConnection.IsVisible = true;
                _btnDelConnection.IsVisible = true;
                _btnExportConnectionFile.IsVisible = true;
                _btnGenerateConnectionUrl.IsVisible = true;
            }
        }

        private static Control CreateTreeItemIcon(string iconData, IBrush iconForeground)
        {
            var pathIcon = new PathIcon
            {
                Data = Geometry.Parse(iconData),
                Width = 16,
                Height = 16,
                Margin = new Avalonia.Thickness(0, 0, 8, 0)
            };
            if (iconForeground != null)
                pathIcon.Foreground = iconForeground;
            return pathIcon;
        }

        private static StackPanel CreateTreeItemHeader(string iconData, string text, IBrush iconForeground = null)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateTreeItemIcon(iconData, iconForeground),
                    new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center }
                },
                Tag = text
            };
        }

        // Semi Design Icons (Material Design paths)
        private const string ICON_CONNECTED = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.95-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z";
        private const string ICON_INSTRUCTION = "M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z";
        private const string ICON_NOTICE = "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z";
        private const string ICON_COMMAND = "M20 19.59V8l-6-6H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c.45 0 .85-.15 1.19-.4l-4.43-4.43c-.8.52-1.74.83-2.76.83-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5c0 1.02-.31 1.96-.83 2.75L20 19.59z";
        private const string ICON_NOTICE_TYPE = "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2zm-2 1H8v-6c0-2.48 1.51-4.5 4-4.5s4 2.02 4 4.5v6z";
        private const string ICON_COMMAND_TYPE = "M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0l4.6-4.6-4.6-4.6L16 6l6 6-6 6-1.4-1.4z";

        private void DisplayInstructions(TreeViewItem connectionNode, QpInstruction[] instructions)
        {
            connectionNode.Items.Clear();
            foreach (var instruction in instructions)
            {
                var instructionNode = new TreeViewItem
                {
                    Header = CreateTreeItemHeader(ICON_INSTRUCTION, instruction.Name),
                    Tag = instruction
                };
                connectionNode.Items.Add(instructionNode);

                if (instruction.NoticeInfos != null)
                {
                    var noticesNode = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader(ICON_NOTICE, "通知"),
                        Tag = instruction.NoticeInfos
                    };
                    instructionNode.Items.Add(noticesNode);
                    foreach (var noticeInfo in instruction.NoticeInfos)
                    {
                        var noticeNode = new TreeViewItem
                        {
                            Header = CreateTreeItemHeader(ICON_NOTICE_TYPE, noticeInfo.Name),
                            Tag = noticeInfo,
                            ContextMenu = _cmsNotice
                        };
                        noticesNode.Items.Add(noticeNode);
                    }
                }
                if (instruction.CommandInfos != null)
                {
                    var commandsNode = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader(ICON_COMMAND, "命令"),
                        Tag = instruction.CommandInfos
                    };
                    instructionNode.Items.Add(commandsNode);
                    foreach (var commandInfo in instruction.CommandInfos)
                    {
                        var commandNode = new TreeViewItem
                        {
                            Header = CreateTreeItemHeader(ICON_COMMAND_TYPE, commandInfo.Name),
                            Tag = commandInfo,
                            ContextMenu = _cmsCommand
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
                Header = CreateTreeItemHeader(ICON_CONNECTED, connectionInfo.Name),
                Tag = new ConnectionContext(connectionInfo),
                ContextMenu = _cmsConnection
            };
            tvQpInstructions.Items.Add(connectionNode);
            if (connectionInfo.Instructions != null)
                DisplayInstructions(connectionNode, connectionInfo.Instructions);
        }

        private async void BtnQuickAddConnection_Click(object sender, RoutedEventArgs e)
        {
            var form = new QuickConnectForm();
            var result = await form.ShowDialog<TestConnectionInfo>(this);
            if (result == null)
                return;
            AddConnection(result);
            QpdFileUtils.SaveQpbFile(result);
        }

        private async void BtnAddConnection_Click(object sender, RoutedEventArgs e)
        {
            var form = new ConnectForm();
            var result = await form.ShowDialog<TestConnectionInfo>(this);
            if (result == null)
                return;
            AddConnection(result);
            QpdFileUtils.SaveQpbFile(result);
        }

        private void BtnDisconnectConnection_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is ConnectionContext connectionContext)
                connectionContext.Dispose();
        }

        private async void BtnDelConnection_Click(object sender, RoutedEventArgs e)
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
                // 更新图标为已连接状态
                connectionNode.Header = CreateTreeItemHeader(ICON_CONNECTED, connectionContext.ConnectionInfo.Name, (IBrush)this.FindResource("SemiColorPrimary"));
                connectionContext.Disconnected += (s, e) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        // 更新图标为未连接状态
                        connectionNode.Header = CreateTreeItemHeader(ICON_CONNECTED, connectionContext.ConnectionInfo.Name);
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

        private async void BtnConnectConnection_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            await ConnectConnectionAsync(node, connectionContext);
        }

        private void BtnRecvHeartbeat_Connection_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new HeartbeatRecvForm(connectionContext);
            form.Show(this);
        }

        private void BtnRecvNotice_Connection_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new NoticeRecvForm(connectionContext);
            form.Show(this);
        }

        private void BtnTestCommand_Connection_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;
            var form = new CommandTestForm(connectionContext);
            form.Show(this);
        }

        private async void BtnEditConnection_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            if (node.Tag is not ConnectionContext connectionContext)
                return;

            var form = new ConnectForm();
            form.EditConnectionInfo(connectionContext.ConnectionInfo);
            var result = await form.ShowDialog<TestConnectionInfo>(this);
            if (result == null)
                return;

            connectionContext.Dispose();
            tvQpInstructions.Items.Remove(node);
            AddConnection(result);
            QpdFileUtils.SaveQpbFile(result);
        }

        private async void BtnExportConnectionFile_Click(object sender, RoutedEventArgs e)
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

        private async void BtnGenerateConnectionUrl_Click(object sender, RoutedEventArgs e)
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

        private void BtnRecvNotice_Notice_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            OpenNoticeRecvForm(node);
        }

        private void BtnTestCommand_Command_Click(object sender, RoutedEventArgs e)
        {
            if (tvQpInstructions.SelectedItem is not TreeViewItem node)
                return;
            OpenCommandTestForm(node);
        }

        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var form = new AboutBox();
            await form.ShowDialog(this);
        }
    }
}
