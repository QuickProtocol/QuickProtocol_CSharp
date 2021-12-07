
namespace QpTestClient.Forms
{
    partial class HeartbeatRecvForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeartbeatRecvForm));
            this.nudMaxLines = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFormTitle = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStopRecv = new System.Windows.Forms.Button();
            this.btnStartRecv = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLines)).BeginInit();
            this.SuspendLayout();
            // 
            // nudMaxLines
            // 
            this.nudMaxLines.Location = new System.Drawing.Point(112, 47);
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
            this.nudMaxLines.TabIndex = 15;
            this.nudMaxLines.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 20);
            this.label3.TabIndex = 12;
            this.label3.Text = "显示行数:";
            // 
            // txtFormTitle
            // 
            this.txtFormTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFormTitle.Location = new System.Drawing.Point(112, 12);
            this.txtFormTitle.Name = "txtFormTitle";
            this.txtFormTitle.Size = new System.Drawing.Size(520, 27);
            this.txtFormTitle.TabIndex = 11;
            this.txtFormTitle.TextChanged += new System.EventHandler(this.txtFormTitle_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "窗口标题:";
            // 
            // btnStopRecv
            // 
            this.btnStopRecv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStopRecv.Enabled = false;
            this.btnStopRecv.Location = new System.Drawing.Point(538, 45);
            this.btnStopRecv.Name = "btnStopRecv";
            this.btnStopRecv.Size = new System.Drawing.Size(94, 29);
            this.btnStopRecv.TabIndex = 8;
            this.btnStopRecv.Text = "停止接收";
            this.btnStopRecv.UseVisualStyleBackColor = true;
            this.btnStopRecv.Click += new System.EventHandler(this.btnStopRecv_Click);
            // 
            // btnStartRecv
            // 
            this.btnStartRecv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartRecv.Location = new System.Drawing.Point(438, 45);
            this.btnStartRecv.Name = "btnStartRecv";
            this.btnStartRecv.Size = new System.Drawing.Size(94, 29);
            this.btnStartRecv.TabIndex = 9;
            this.btnStartRecv.Text = "开始接收";
            this.btnStartRecv.UseVisualStyleBackColor = true;
            this.btnStartRecv.Click += new System.EventHandler(this.btnStartRecv_Click);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 80);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(620, 498);
            this.txtLog.TabIndex = 7;
            // 
            // HeartbeatRecvForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 590);
            this.Controls.Add(this.nudMaxLines);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtFormTitle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStopRecv);
            this.Controls.Add(this.btnStartRecv);
            this.Controls.Add(this.txtLog);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HeartbeatRecvForm";
            this.Text = "心跳接收";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HeartbeatRecvForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxLines)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudMaxLines;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFormTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStopRecv;
        private System.Windows.Forms.Button btnStartRecv;
        private System.Windows.Forms.TextBox txtLog;
    }
}