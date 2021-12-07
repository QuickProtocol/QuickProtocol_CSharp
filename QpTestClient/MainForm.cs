using QpTestClient.Utils;
using Quick.Protocol;
using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient
{
    public partial class MainForm : Form
    {
        public const string QPDFILE_FILTER = "Quick.Protocol连接文件(*.qpd)|*.qpd";

        private TreeNodeCollection treeNodeCollection;

        public MainForm()
        {
            InitializeComponent();
            Text = Application.ProductName;
            treeNodeCollection = tvQpInstructions.Nodes;
            cmsConnection.Opening += CmsConnection_Opening;
            btnAddConnection.Click += BtnAddConnection_Click;
            btnImportConnectionFile.Click += BtnImportConnectionFile_Click;
            btnExit.Click += BtnExit_Click;
            //连接相关
            btnDisconnectConnection.Click += BtnDisconnectConnection_Click;
            btnConnectConnection.Click += BtnConnectConnection_Click;
            btnRecvHeartbeat_Connection.Click += BtnRecvHeartbeat_Connection_Click;
            btnRecvNotice_Connection.Click += BtnRecvNotice_Connection_Click;
            btnTestCommand_Connection.Click += BtnTestCommand_Connection_Click;
            btnEditConnection.Click += BtnEditConnection_Click;
            btnDelConnection.Click += BtnDelConnection_Click;
            btnExportConnectionFile.Click += BtnExportConnectionFile_Click;
            //通知相关
            btnRecvNotice_Notice.Click += BtnRecvNotice_Notice_Click;
            //命令相关
            btnTestCommand_Command.Click += BtnTestCommand_Command_Click;

            this.Text += $" ver:{ProductInfoUtils.GetAssemblyVersion()}";
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            var connectionInfos = QpdFileUtils.GetConnectionInfosFromQpbFileFolder();
            if (connectionInfos != null)
            {
                foreach (var connectionInfo in connectionInfos)
                    addConnection(connectionInfo);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (TreeNode connectNode in treeNodeCollection)
            {
                var connectionContext = (ConnectionContext)connectNode.Tag;
                connectionContext.Dispose();
            }
        }

        private void BtnImportConnectionFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = QPDFILE_FILTER;
            var ret = ofd.ShowDialog();
            if (ret == DialogResult.Cancel)
                return;
            try
            {
                var file = ofd.FileName;
                ConnectionInfo connectionInfo = QpdFileUtils.Load(file);
                connectionInfo.Name = Path.GetFileNameWithoutExtension(file);
                addConnection(connectionInfo);
                QpdFileUtils.SaveQpbFile(connectionInfo);
                MessageBox.Show("导入成功！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void showContent(Control item)
        {
            gbNodeInfo.Controls.Clear();
            if (item != null)
            {
                item.Dock = DockStyle.Fill;
                gbNodeInfo.Controls.Add(item);
            }
        }

        private Control getPropertyGridControl(object obj)
        {
            TypeDescriptor.AddAttributes(obj, new Attribute[] { new ReadOnlyAttribute(true) });

            var control = new PropertyGrid();
            control.SelectedObject = obj;
            control.ToolbarVisible = false;
            control.PropertySort = PropertySort.NoSort;
            return control;
        }

        private void tvQpInstructions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;
            var nodeObj = node.Tag;

            gbNodeInfo.Text = node.Text;
            if (nodeObj == null)
            {
                showContent(null);
            }
            else if (nodeObj is ConnectionContext)
            {
                var connectionContext = (ConnectionContext)nodeObj;
                showContent(getPropertyGridControl(connectionContext.ConnectionInfo.QpClientOptions));
            }
            else if (nodeObj is QpInstruction)
            {
                showContent(getPropertyGridControl(nodeObj));
            }
            else if (nodeObj is QpNoticeInfo)
            {
                showContent(new Controls.NoticeInfoControl((QpNoticeInfo)nodeObj));
            }
            else if (nodeObj is QpCommandInfo)
            {
                showContent(new Controls.CommandInfoControl((QpCommandInfo)nodeObj));
            }
        }

        private void tvQpInstructions_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;
            var nodeObj = node.Tag;

            if (nodeObj == null)
                return;

            if (nodeObj is ConnectionContext)
            {
                var connectionContext = (ConnectionContext)nodeObj;
                if (!connectionContext.Connected)
                    BtnConnectConnection_Click(sender, e);
            }
            else if (nodeObj is QpNoticeInfo)
            {
                BtnRecvNotice_Notice_Click(sender, e);
            }
            else if (nodeObj is QpCommandInfo)
            {
                BtnTestCommand_Command_Click(sender, e);
            }
        }

        private ConnectionContext findConnectionContext(TreeNode treeNode)
        {
            ConnectionContext connectionContext = null;
            var currentNode = treeNode;
            while (currentNode != null)
            {
                if (currentNode.Tag is ConnectionContext)
                {
                    connectionContext = (ConnectionContext)currentNode.Tag;
                    break;
                }
                currentNode = currentNode.Parent;
            }
            return connectionContext;
        }

        private void CmsConnection_Opening(object sender, CancelEventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            if (connectionNode == null)
            {
                e.Cancel = true;
                return;
            }
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
            {
                e.Cancel = true;
                return;
            }
            if (connectionContext.Connected)
            {
                btnConnectConnection.Visible = false;
                btnDisconnectConnection.Visible = true;
                separatorConnection.Visible = true;
                btnRecvHeartbeat_Connection.Visible = true;
                btnRecvNotice_Connection.Visible = true;
                btnTestCommand_Connection.Visible = true;
                btnEditConnection.Visible = false;
                btnDelConnection.Visible = false;
            }
            else
            {
                btnConnectConnection.Visible = true;
                btnDisconnectConnection.Visible = false;
                separatorConnection.Visible = false;
                btnRecvHeartbeat_Connection.Visible = false;
                btnRecvNotice_Connection.Visible = false;
                btnTestCommand_Connection.Visible = false;
                btnEditConnection.Visible = true;
                btnDelConnection.Visible = true;
            }
        }

        private void displayInstructions(TreeNode connectionNode, QpInstruction[] instructions)
        {
            connectionNode.Nodes.Clear();
            foreach (var instruction in instructions)
            {
                var instructionNode = connectionNode.Nodes.Add(instruction.Id, instruction.Name, 2, 2);
                instructionNode.Tag = instruction;
                var noticesNode = instructionNode.Nodes.Add("Notice", "通知", 3, 3);
                foreach (var noticeInfo in instruction.NoticeInfos)
                {
                    var noticeNode = noticesNode.Nodes.Add(noticeInfo.NoticeTypeName, noticeInfo.Name, 3, 3);
                    noticeNode.ContextMenuStrip = cmsNotice;
                    noticeNode.Tag = noticeInfo;
                }
                var commandsNode = instructionNode.Nodes.Add("Command", "命令", 4, 4);
                foreach (var commandInfo in instruction.CommandInfos)
                {
                    var commandNode = commandsNode.Nodes.Add(commandInfo.RequestTypeName, commandInfo.Name, 4, 4);
                    commandNode.ContextMenuStrip = cmsCommand;
                    commandNode.Tag = commandInfo;
                }
            }
        }

        private void addConnection(ConnectionInfo connectionInfo)
        {
            var connectionNode = treeNodeCollection.Add(connectionInfo.Name, connectionInfo.Name, 0, 0);
            connectionNode.Tag = new ConnectionContext(connectionInfo);
            connectionNode.ContextMenuStrip = cmsConnection;
            if (connectionInfo.Instructions != null)
                displayInstructions(connectionNode, connectionInfo.Instructions);
        }

        private void BtnAddConnection_Click(object sender, EventArgs e)
        {
            var form = new ConnectForm();
            var ret = form.ShowDialog();
            if (ret != DialogResult.OK)
                return;
            var connectionInfo = form.ConnectionInfo;
            addConnection(connectionInfo);
            QpdFileUtils.SaveQpbFile(connectionInfo);
        }


        private void BtnDisconnectConnection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;
            connectionContext.Dispose();
        }

        private void BtnDelConnection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            var ret = MessageBox.Show($"确定要删除连接[{connectionContext.ConnectionInfo.Name}]?", "删除确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (ret == DialogResult.Cancel)
                return;

            connectionContext.Dispose();
            treeNodeCollection.Remove(connectionNode);
            QpdFileUtils.DeleteQpbFile(connectionContext.ConnectionInfo);
        }

        private async void BtnConnectConnection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            this.Enabled = false;
            try
            {
                var preConnectionInfoContent = Quick.Xml.XmlConvert.Serialize(connectionContext.ConnectionInfo);

                await connectionContext.Connect();
                connectionNode.ImageIndex = connectionNode.SelectedImageIndex = 1;
                connectionContext.QpClient.Disconnected += (sender, e) =>
                {
                    Invoke(new Action(() =>
                    {
                        connectionNode.ImageIndex = connectionNode.SelectedImageIndex = 0;
                        connectionContext.Dispose();
                    }));
                };
                displayInstructions(connectionNode, connectionContext.ConnectionInfo.Instructions);
                connectionNode.ExpandAll();

                var currentConnectionInfoContent = Quick.Xml.XmlConvert.Serialize(connectionContext.ConnectionInfo);
                if (currentConnectionInfoContent != preConnectionInfoContent)
                    QpdFileUtils.SaveQpbFile(connectionContext.ConnectionInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            this.Enabled = true;
        }

        private void BtnRecvHeartbeat_Connection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            var form = new Forms.HeartbeatRecvForm(connectionContext);
            form.Show();
        }

        private void BtnRecvNotice_Connection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            var form = new Forms.NoticeRecvForm(connectionContext);
            form.Show();
        }

        private void BtnTestCommand_Connection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            var form = new Forms.CommandTestForm(connectionContext);
            form.Show();
        }

        private void BtnEditConnection_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            var form = new ConnectForm();
            form.EditConnectionInfo(connectionContext.ConnectionInfo);
            var ret = form.ShowDialog();
            if (ret != DialogResult.OK)
                return;
            var connectionInfo = form.ConnectionInfo;
            connectionContext.Dispose();
            treeNodeCollection.Remove(connectionNode);

            addConnection(connectionInfo);
            QpdFileUtils.SaveQpbFile(connectionInfo);
        }

        private void BtnExportConnectionFile_Click(object sender, EventArgs e)
        {
            var connectionNode = tvQpInstructions.SelectedNode;
            var connectionContext = connectionNode.Tag as ConnectionContext;
            if (connectionContext == null)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = QPDFILE_FILTER;
            var ret = sfd.ShowDialog();
            if (ret == DialogResult.Cancel)
                return;
            var file = sfd.FileName;
            QpdFileUtils.SaveQpbFile(connectionContext.ConnectionInfo, file);
            MessageBox.Show("导出成功！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRecvNotice_Notice_Click(object sender, EventArgs e)
        {
            var noticeNode = tvQpInstructions.SelectedNode;
            var qpNoticeInfo = noticeNode.Tag as QpNoticeInfo;
            if (qpNoticeInfo == null)
                return;

            ConnectionContext connectionContext = findConnectionContext(noticeNode);
            var form = new Forms.NoticeRecvForm(connectionContext, qpNoticeInfo);
            form.Show();
        }

        private void BtnTestCommand_Command_Click(object sender, EventArgs e)
        {
            var commandNode = tvQpInstructions.SelectedNode;
            var qpCommandInfo = commandNode.Tag as QpCommandInfo;
            if (qpCommandInfo == null)
                return;

            ConnectionContext connectionContext = findConnectionContext(commandNode);
            var form = new Forms.CommandTestForm(connectionContext, qpCommandInfo);
            form.Show();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }
    }
}
