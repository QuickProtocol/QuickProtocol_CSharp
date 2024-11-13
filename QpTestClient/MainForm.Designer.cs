
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tsMain = new System.Windows.Forms.ToolStrip();
            btnFile = new System.Windows.Forms.ToolStripDropDownButton();
            btnQuickAddConnection = new System.Windows.Forms.ToolStripMenuItem();
            btnAddConnection = new System.Windows.Forms.ToolStripMenuItem();
            btnImportConnectionFile = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnExit = new System.Windows.Forms.ToolStripMenuItem();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scMain = new System.Windows.Forms.SplitContainer();
            tvQpInstructions = new System.Windows.Forms.TreeView();
            ilQpInstructions = new System.Windows.Forms.ImageList(components);
            gbNodeInfo = new System.Windows.Forms.GroupBox();
            cmsConnection = new System.Windows.Forms.ContextMenuStrip(components);
            btnDisconnectConnection = new System.Windows.Forms.ToolStripMenuItem();
            btnConnectConnection = new System.Windows.Forms.ToolStripMenuItem();
            separatorConnection = new System.Windows.Forms.ToolStripSeparator();
            btnRecvHeartbeat_Connection = new System.Windows.Forms.ToolStripMenuItem();
            btnRecvNotice_Connection = new System.Windows.Forms.ToolStripMenuItem();
            btnTestCommand_Connection = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            btnEditConnection = new System.Windows.Forms.ToolStripMenuItem();
            btnDelConnection = new System.Windows.Forms.ToolStripMenuItem();
            btnExportConnectionFile = new System.Windows.Forms.ToolStripMenuItem();
            btnGenerateConnectionUrl = new System.Windows.Forms.ToolStripMenuItem();
            cmsNotice = new System.Windows.Forms.ContextMenuStrip(components);
            btnRecvNotice_Notice = new System.Windows.Forms.ToolStripMenuItem();
            cmsCommand = new System.Windows.Forms.ContextMenuStrip(components);
            btnTestCommand_Command = new System.Windows.Forms.ToolStripMenuItem();
            tsMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scMain).BeginInit();
            scMain.Panel1.SuspendLayout();
            scMain.Panel2.SuspendLayout();
            scMain.SuspendLayout();
            cmsConnection.SuspendLayout();
            cmsNotice.SuspendLayout();
            cmsCommand.SuspendLayout();
            SuspendLayout();
            // 
            // tsMain
            // 
            tsMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnFile, toolStripDropDownButton1 });
            tsMain.Location = new System.Drawing.Point(0, 0);
            tsMain.Name = "tsMain";
            tsMain.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            tsMain.Size = new System.Drawing.Size(1296, 41);
            tsMain.TabIndex = 2;
            tsMain.Text = "toolStrip1";
            // 
            // btnFile
            // 
            btnFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { btnQuickAddConnection, btnAddConnection, btnImportConnectionFile, toolStripSeparator1, btnExit });
            btnFile.Image = (System.Drawing.Image)resources.GetObject("btnFile.Image");
            btnFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnFile.Name = "btnFile";
            btnFile.Size = new System.Drawing.Size(113, 35);
            btnFile.Text = "文件(&F)";
            // 
            // btnQuickAddConnection
            // 
            btnQuickAddConnection.Name = "btnQuickAddConnection";
            btnQuickAddConnection.Size = new System.Drawing.Size(297, 44);
            btnQuickAddConnection.Text = "快速添加(&Q)...";
            // 
            // btnAddConnection
            // 
            btnAddConnection.Name = "btnAddConnection";
            btnAddConnection.Size = new System.Drawing.Size(297, 44);
            btnAddConnection.Text = "添加(&A)...";
            // 
            // btnImportConnectionFile
            // 
            btnImportConnectionFile.Name = "btnImportConnectionFile";
            btnImportConnectionFile.Size = new System.Drawing.Size(297, 44);
            btnImportConnectionFile.Text = "导入(&I)..";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(294, 6);
            // 
            // btnExit
            // 
            btnExit.Name = "btnExit";
            btnExit.Size = new System.Drawing.Size(297, 44);
            btnExit.Text = "退出(&X)";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { 关于ToolStripMenuItem });
            toolStripDropDownButton1.Image = (System.Drawing.Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(119, 35);
            toolStripDropDownButton1.Text = "帮助(&H)";
            // 
            // 关于ToolStripMenuItem
            // 
            关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            关于ToolStripMenuItem.Size = new System.Drawing.Size(228, 44);
            关于ToolStripMenuItem.Text = "关于(&A)";
            关于ToolStripMenuItem.Click += 关于ToolStripMenuItem_Click;
            // 
            // scMain
            // 
            scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            scMain.Location = new System.Drawing.Point(0, 41);
            scMain.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            scMain.Name = "scMain";
            // 
            // scMain.Panel1
            // 
            scMain.Panel1.Controls.Add(tvQpInstructions);
            // 
            // scMain.Panel2
            // 
            scMain.Panel2.Controls.Add(gbNodeInfo);
            scMain.Size = new System.Drawing.Size(1296, 794);
            scMain.SplitterDistance = 427;
            scMain.SplitterWidth = 6;
            scMain.TabIndex = 5;
            // 
            // tvQpInstructions
            // 
            tvQpInstructions.Dock = System.Windows.Forms.DockStyle.Fill;
            tvQpInstructions.HideSelection = false;
            tvQpInstructions.ImageIndex = 0;
            tvQpInstructions.ImageList = ilQpInstructions;
            tvQpInstructions.Location = new System.Drawing.Point(0, 0);
            tvQpInstructions.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tvQpInstructions.Name = "tvQpInstructions";
            tvQpInstructions.SelectedImageIndex = 0;
            tvQpInstructions.Size = new System.Drawing.Size(427, 794);
            tvQpInstructions.TabIndex = 0;
            tvQpInstructions.AfterSelect += tvQpInstructions_AfterSelect;
            tvQpInstructions.NodeMouseDoubleClick += tvQpInstructions_NodeMouseDoubleClick;
            // 
            // ilQpInstructions
            // 
            ilQpInstructions.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            ilQpInstructions.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // gbNodeInfo
            // 
            gbNodeInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            gbNodeInfo.Location = new System.Drawing.Point(0, 0);
            gbNodeInfo.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            gbNodeInfo.Name = "gbNodeInfo";
            gbNodeInfo.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            gbNodeInfo.Size = new System.Drawing.Size(863, 794);
            gbNodeInfo.TabIndex = 0;
            gbNodeInfo.TabStop = false;
            // 
            // cmsConnection
            // 
            cmsConnection.ImageScalingSize = new System.Drawing.Size(20, 20);
            cmsConnection.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnDisconnectConnection, btnConnectConnection, separatorConnection, btnRecvHeartbeat_Connection, btnRecvNotice_Connection, btnTestCommand_Connection, toolStripSeparator3, btnEditConnection, btnDelConnection, btnExportConnectionFile, btnGenerateConnectionUrl });
            cmsConnection.Name = "cmsConnection";
            cmsConnection.Size = new System.Drawing.Size(232, 358);
            // 
            // btnDisconnectConnection
            // 
            btnDisconnectConnection.Name = "btnDisconnectConnection";
            btnDisconnectConnection.Size = new System.Drawing.Size(231, 38);
            btnDisconnectConnection.Text = "断开(&D)";
            // 
            // btnConnectConnection
            // 
            btnConnectConnection.Name = "btnConnectConnection";
            btnConnectConnection.Size = new System.Drawing.Size(231, 38);
            btnConnectConnection.Text = "连接";
            // 
            // separatorConnection
            // 
            separatorConnection.Name = "separatorConnection";
            separatorConnection.Size = new System.Drawing.Size(228, 6);
            // 
            // btnRecvHeartbeat_Connection
            // 
            btnRecvHeartbeat_Connection.Name = "btnRecvHeartbeat_Connection";
            btnRecvHeartbeat_Connection.Size = new System.Drawing.Size(231, 38);
            btnRecvHeartbeat_Connection.Text = "接收心跳(&H)..";
            // 
            // btnRecvNotice_Connection
            // 
            btnRecvNotice_Connection.Name = "btnRecvNotice_Connection";
            btnRecvNotice_Connection.Size = new System.Drawing.Size(231, 38);
            btnRecvNotice_Connection.Text = "接收通知(&R)..";
            // 
            // btnTestCommand_Connection
            // 
            btnTestCommand_Connection.Name = "btnTestCommand_Connection";
            btnTestCommand_Connection.Size = new System.Drawing.Size(231, 38);
            btnTestCommand_Connection.Text = "测试命令(&T)..";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(228, 6);
            // 
            // btnEditConnection
            // 
            btnEditConnection.Name = "btnEditConnection";
            btnEditConnection.Size = new System.Drawing.Size(231, 38);
            btnEditConnection.Text = "编辑(&E)..";
            // 
            // btnDelConnection
            // 
            btnDelConnection.Name = "btnDelConnection";
            btnDelConnection.Size = new System.Drawing.Size(231, 38);
            btnDelConnection.Text = "删除(&D)";
            // 
            // btnExportConnectionFile
            // 
            btnExportConnectionFile.Name = "btnExportConnectionFile";
            btnExportConnectionFile.Size = new System.Drawing.Size(231, 38);
            btnExportConnectionFile.Text = "导出(&X)..";
            // 
            // btnGenerateConnectionUrl
            // 
            btnGenerateConnectionUrl.Name = "btnGenerateConnectionUrl";
            btnGenerateConnectionUrl.Size = new System.Drawing.Size(231, 38);
            btnGenerateConnectionUrl.Text = "生成URL(&U)..";
            btnGenerateConnectionUrl.Click += btnGenerateConnectionUrl_Click;
            // 
            // cmsNotice
            // 
            cmsNotice.ImageScalingSize = new System.Drawing.Size(20, 20);
            cmsNotice.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnRecvNotice_Notice });
            cmsNotice.Name = "cmsNotice";
            cmsNotice.Size = new System.Drawing.Size(229, 42);
            // 
            // btnRecvNotice_Notice
            // 
            btnRecvNotice_Notice.Name = "btnRecvNotice_Notice";
            btnRecvNotice_Notice.Size = new System.Drawing.Size(228, 38);
            btnRecvNotice_Notice.Text = "接收通知(&R)..";
            // 
            // cmsCommand
            // 
            cmsCommand.ImageScalingSize = new System.Drawing.Size(20, 20);
            cmsCommand.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnTestCommand_Command });
            cmsCommand.Name = "cmsCommand";
            cmsCommand.Size = new System.Drawing.Size(179, 42);
            // 
            // btnTestCommand_Command
            // 
            btnTestCommand_Command.Name = "btnTestCommand_Command";
            btnTestCommand_Command.Size = new System.Drawing.Size(178, 38);
            btnTestCommand_Command.Text = "测试(&T)..";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1296, 835);
            Controls.Add(scMain);
            Controls.Add(tsMain);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "MainForm";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            tsMain.ResumeLayout(false);
            tsMain.PerformLayout();
            scMain.Panel1.ResumeLayout(false);
            scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scMain).EndInit();
            scMain.ResumeLayout(false);
            cmsConnection.ResumeLayout(false);
            cmsNotice.ResumeLayout(false);
            cmsCommand.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem btnQuickAddConnection;
        private System.Windows.Forms.ToolStripMenuItem btnGenerateConnectionUrl;
    }
}

