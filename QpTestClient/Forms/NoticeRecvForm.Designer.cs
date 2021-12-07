
namespace QpTestClient.Forms
{
    partial class NoticeRecvForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NoticeRecvForm));
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnStartRecv = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFormTitle = new System.Windows.Forms.TextBox();
            this.txtNoticeTypeName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nudMaxLines = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStopRecv = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLines)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 113);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(620, 465);
            this.txtLog.TabIndex = 0;
            // 
            // btnStartRecv
            // 
            this.btnStartRecv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartRecv.Location = new System.Drawing.Point(438, 78);
            this.btnStartRecv.Name = "btnStartRecv";
            this.btnStartRecv.Size = new System.Drawing.Size(94, 29);
            this.btnStartRecv.TabIndex = 1;
            this.btnStartRecv.Text = "开始接收";
            this.btnStartRecv.UseVisualStyleBackColor = true;
            this.btnStartRecv.Click += new System.EventHandler(this.btnStartRecv_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "窗口标题:";
            // 
            // txtFormTitle
            // 
            this.txtFormTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFormTitle.Location = new System.Drawing.Point(112, 12);
            this.txtFormTitle.Name = "txtFormTitle";
            this.txtFormTitle.Size = new System.Drawing.Size(520, 27);
            this.txtFormTitle.TabIndex = 3;
            this.txtFormTitle.TextChanged += new System.EventHandler(this.txtFormTitle_TextChanged);
            // 
            // txtNoticeTypeName
            // 
            this.txtNoticeTypeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNoticeTypeName.Location = new System.Drawing.Point(112, 45);
            this.txtNoticeTypeName.Name = "txtNoticeTypeName";
            this.txtNoticeTypeName.Size = new System.Drawing.Size(520, 27);
            this.txtNoticeTypeName.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "通知类型:";
            // 
            // nudMaxLines
            // 
            this.nudMaxLines.Location = new System.Drawing.Point(112, 80);
            this.nudMaxLines.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudMaxLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxLines.Name = "nudMaxLines";
            this.nudMaxLines.Size = new System.Drawing.Size(150, 27);
            this.nudMaxLines.TabIndex = 6;
            this.nudMaxLines.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "显示行数:";
            // 
            // btnStopRecv
            // 
            this.btnStopRecv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStopRecv.Enabled = false;
            this.btnStopRecv.Location = new System.Drawing.Point(538, 78);
            this.btnStopRecv.Name = "btnStopRecv";
            this.btnStopRecv.Size = new System.Drawing.Size(94, 29);
            this.btnStopRecv.TabIndex = 1;
            this.btnStopRecv.Text = "停止接收";
            this.btnStopRecv.UseVisualStyleBackColor = true;
            this.btnStopRecv.Click += new System.EventHandler(this.btnStopRecv_Click);
            // 
            // NoticeRecvForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 590);
            this.Controls.Add(this.nudMaxLines);
            this.Controls.Add(this.txtNoticeTypeName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFormTitle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStopRecv);
            this.Controls.Add(this.btnStartRecv);
            this.Controls.Add(this.txtLog);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NoticeRecvForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "通知接收";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NoticeRecvForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLines)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnStartRecv;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFormTitle;
        private System.Windows.Forms.TextBox txtNoticeTypeName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudMaxLines;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnStopRecv;
    }
}