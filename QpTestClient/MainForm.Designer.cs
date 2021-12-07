
namespace QpTestClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.btnFile = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnAddConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.btnImportConnectionFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExit = new System.Windows.Forms.ToolStripMenuItem();
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.tvQpInstructions = new System.Windows.Forms.TreeView();
            this.ilQpInstructions = new System.Windows.Forms.ImageList(this.components);
            this.gbNodeInfo = new System.Windows.Forms.GroupBox();
            this.cmsConnection = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnDisconnectConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.btnConnectConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.separatorConnection = new System.Windows.Forms.ToolStripSeparator();
            this.btnRecvHeartbeat_Connection = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRecvNotice_Connection = new System.Windows.Forms.ToolStripMenuItem();
            this.btnTestCommand_Connection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEditConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.btnDelConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportConnectionFile = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsNotice = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnRecvNotice_Notice = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCommand = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnTestCommand_Command = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            this.cmsConnection.SuspendLayout();
            this.cmsNotice.SuspendLayout();
            this.cmsCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsMain
            // 
            this.tsMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnFile,
            this.toolStripDropDownButton1});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(833, 27);
            this.tsMain.TabIndex = 2;
            this.tsMain.Text = "toolStrip1";
            // 
            // btnFile
            // 
            this.btnFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddConnection,
            this.btnImportConnectionFile,
            this.toolStripSeparator1,
            this.btnExit});
            this.btnFile.Image = ((System.Drawing.Image)(resources.GetObject("btnFile.Image")));
            this.btnFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(71, 24);
            this.btnFile.Text = "文件(&F)";
            // 
            // btnAddConnection
            // 
            this.btnAddConnection.Name = "btnAddConnection";
            this.btnAddConnection.Size = new System.Drawing.Size(224, 26);
            this.btnAddConnection.Text = "添加(&A)...";
            // 
            // btnImportConnectionFile
            // 
            this.btnImportConnectionFile.Name = "btnImportConnectionFile";
            this.btnImportConnectionFile.Size = new System.Drawing.Size(224, 26);
            this.btnImportConnectionFile.Text = "导入(&I)..";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(221, 6);
            // 
            // btnExit
            // 
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(224, 26);
            this.btnExit.Text = "退出(&X)";
            // 
            // scMain
            // 
            this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMain.Location = new System.Drawing.Point(0, 27);
            this.scMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.scMain.Name = "scMain";
            // 
            // scMain.Panel1
            // 
            this.scMain.Panel1.Controls.Add(this.tvQpInstructions);
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.gbNodeInfo);
            this.scMain.Size = new System.Drawing.Size(833, 512);
            this.scMain.SplitterDistance = 276;
            this.scMain.TabIndex = 5;
            // 
            // tvQpInstructions
            // 
            this.tvQpInstructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvQpInstructions.HideSelection = false;
            this.tvQpInstructions.ImageIndex = 0;
            this.tvQpInstructions.ImageList = this.ilQpInstructions;
            this.tvQpInstructions.Location = new System.Drawing.Point(0, 0);
            this.tvQpInstructions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tvQpInstructions.Name = "tvQpInstructions";
            this.tvQpInstructions.SelectedImageIndex = 0;
            this.tvQpInstructions.Size = new System.Drawing.Size(276, 512);
            this.tvQpInstructions.TabIndex = 0;
            this.tvQpInstructions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvQpInstructions_AfterSelect);
            this.tvQpInstructions.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvQpInstructions_NodeMouseDoubleClick);
            // 
            // ilQpInstructions
            // 
            this.ilQpInstructions.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ilQpInstructions.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilQpInstructions.ImageStream")));
            this.ilQpInstructions.TransparentColor = System.Drawing.Color.Transparent;
            this.ilQpInstructions.Images.SetKeyName(0, "connect_gray.png");
            this.ilQpInstructions.Images.SetKeyName(1, "connect.png");
            this.ilQpInstructions.Images.SetKeyName(2, "archive_16px_1092529_easyicon.net.png");
            this.ilQpInstructions.Images.SetKeyName(3, "speaker_volume_16px_514569_easyicon.net.png");
            this.ilQpInstructions.Images.SetKeyName(4, "command_16px_583324_easyicon.net.png");
            // 
            // gbNodeInfo
            // 
            this.gbNodeInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbNodeInfo.Location = new System.Drawing.Point(0, 0);
            this.gbNodeInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbNodeInfo.Name = "gbNodeInfo";
            this.gbNodeInfo.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbNodeInfo.Size = new System.Drawing.Size(553, 512);
            this.gbNodeInfo.TabIndex = 0;
            this.gbNodeInfo.TabStop = false;
            // 
            // cmsConnection
            // 
            this.cmsConnection.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsConnection.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnDisconnectConnection,
            this.btnConnectConnection,
            this.separatorConnection,
            this.btnRecvHeartbeat_Connection,
            this.btnRecvNotice_Connection,
            this.btnTestCommand_Connection,
            this.toolStripSeparator3,
            this.btnEditConnection,
            this.btnDelConnection,
            this.btnExportConnectionFile});
            this.cmsConnection.Name = "cmsConnection";
            this.cmsConnection.Size = new System.Drawing.Size(169, 208);
            // 
            // btnDisconnectConnection
            // 
            this.btnDisconnectConnection.Name = "btnDisconnectConnection";
            this.btnDisconnectConnection.Size = new System.Drawing.Size(168, 24);
            this.btnDisconnectConnection.Text = "断开(&D)";
            // 
            // btnConnectConnection
            // 
            this.btnConnectConnection.Name = "btnConnectConnection";
            this.btnConnectConnection.Size = new System.Drawing.Size(168, 24);
            this.btnConnectConnection.Text = "连接";
            // 
            // separatorConnection
            // 
            this.separatorConnection.Name = "separatorConnection";
            this.separatorConnection.Size = new System.Drawing.Size(165, 6);
            // 
            // btnRecvHeartbeat_Connection
            // 
            this.btnRecvHeartbeat_Connection.Name = "btnRecvHeartbeat_Connection";
            this.btnRecvHeartbeat_Connection.Size = new System.Drawing.Size(168, 24);
            this.btnRecvHeartbeat_Connection.Text = "接收心跳(&H)..";
            // 
            // btnRecvNotice_Connection
            // 
            this.btnRecvNotice_Connection.Name = "btnRecvNotice_Connection";
            this.btnRecvNotice_Connection.Size = new System.Drawing.Size(168, 24);
            this.btnRecvNotice_Connection.Text = "接收通知(&R)..";
            // 
            // btnTestCommand_Connection
            // 
            this.btnTestCommand_Connection.Name = "btnTestCommand_Connection";
            this.btnTestCommand_Connection.Size = new System.Drawing.Size(168, 24);
            this.btnTestCommand_Connection.Text = "测试命令(&T)..";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(165, 6);
            // 
            // btnEditConnection
            // 
            this.btnEditConnection.Name = "btnEditConnection";
            this.btnEditConnection.Size = new System.Drawing.Size(168, 24);
            this.btnEditConnection.Text = "编辑(&E)..";
            // 
            // btnDelConnection
            // 
            this.btnDelConnection.Name = "btnDelConnection";
            this.btnDelConnection.Size = new System.Drawing.Size(168, 24);
            this.btnDelConnection.Text = "删除(&D)";
            // 
            // btnExportConnectionFile
            // 
            this.btnExportConnectionFile.Name = "btnExportConnectionFile";
            this.btnExportConnectionFile.Size = new System.Drawing.Size(168, 24);
            this.btnExportConnectionFile.Text = "导出(&X)..";
            // 
            // cmsNotice
            // 
            this.cmsNotice.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsNotice.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRecvNotice_Notice});
            this.cmsNotice.Name = "cmsNotice";
            this.cmsNotice.Size = new System.Drawing.Size(167, 28);
            // 
            // btnRecvNotice_Notice
            // 
            this.btnRecvNotice_Notice.Name = "btnRecvNotice_Notice";
            this.btnRecvNotice_Notice.Size = new System.Drawing.Size(166, 24);
            this.btnRecvNotice_Notice.Text = "接收通知(&R)..";
            // 
            // cmsCommand
            // 
            this.cmsCommand.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsCommand.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnTestCommand_Command});
            this.cmsCommand.Name = "cmsCommand";
            this.cmsCommand.Size = new System.Drawing.Size(136, 28);
            // 
            // btnTestCommand_Command
            // 
            this.btnTestCommand_Command.Name = "btnTestCommand_Command";
            this.btnTestCommand_Command.Size = new System.Drawing.Size(135, 24);
            this.btnTestCommand_Command.Text = "测试(&T)..";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.关于ToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(75, 24);
            this.toolStripDropDownButton1.Text = "帮助(&H)";
            // 
            // 关于ToolStripMenuItem
            // 
            this.关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            this.关于ToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.关于ToolStripMenuItem.Text = "关于(&A)";
            this.关于ToolStripMenuItem.Click += new System.EventHandler(this.关于ToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 539);
            this.Controls.Add(this.scMain);
            this.Controls.Add(this.tsMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            this.cmsConnection.ResumeLayout(false);
            this.cmsNotice.ResumeLayout(false);
            this.cmsCommand.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.TreeView tvQpInstructions;
        private System.Windows.Forms.ImageList ilQpInstructions;
        private System.Windows.Forms.GroupBox gbNodeInfo;
        private System.Windows.Forms.ToolStripDropDownButton btnFile;
        private System.Windows.Forms.ToolStripMenuItem btnAddConnection;
        private System.Windows.Forms.ContextMenuStrip cmsConnection;
        private System.Windows.Forms.ToolStripMenuItem btnDisconnectConnection;
        private System.Windows.Forms.ToolStripMenuItem btnImportConnectionFile;
        private System.Windows.Forms.ToolStripMenuItem btnExit;
        private System.Windows.Forms.ToolStripMenuItem btnConnectConnection;
        private System.Windows.Forms.ToolStripMenuItem btnDelConnection;
        private System.Windows.Forms.ToolStripMenuItem btnExportConnectionFile;
        private System.Windows.Forms.ToolStripMenuItem btnEditConnection;
        private System.Windows.Forms.ContextMenuStrip cmsNotice;
        private System.Windows.Forms.ToolStripMenuItem btnRecvNotice_Notice;
        private System.Windows.Forms.ContextMenuStrip cmsCommand;
        private System.Windows.Forms.ToolStripMenuItem btnTestCommand_Command;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator separatorConnection;
        private System.Windows.Forms.ToolStripMenuItem btnRecvNotice_Connection;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem btnTestCommand_Connection;
        private System.Windows.Forms.ToolStripMenuItem btnRecvHeartbeat_Connection;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem 关于ToolStripMenuItem;
    }
}

