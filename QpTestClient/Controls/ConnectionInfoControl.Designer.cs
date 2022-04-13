namespace QpTestClient.Controls
{
    partial class ConnectionInfoControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpBasic = new System.Windows.Forms.TabPage();
            this.tpNetstat = new System.Windows.Forms.TabPage();
            this.txtNetstat = new System.Windows.Forms.TextBox();
            this.timerNetstat = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tpNetstat.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpBasic);
            this.tabControl1.Controls.Add(this.tpNetstat);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(533, 426);
            this.tabControl1.TabIndex = 1;
            // 
            // tpBasic
            // 
            this.tpBasic.Location = new System.Drawing.Point(4, 29);
            this.tpBasic.Name = "tpBasic";
            this.tpBasic.Padding = new System.Windows.Forms.Padding(3);
            this.tpBasic.Size = new System.Drawing.Size(525, 393);
            this.tpBasic.TabIndex = 0;
            this.tpBasic.Text = "基本";
            this.tpBasic.UseVisualStyleBackColor = true;
            // 
            // tpNetstat
            // 
            this.tpNetstat.Controls.Add(this.txtNetstat);
            this.tpNetstat.Location = new System.Drawing.Point(4, 29);
            this.tpNetstat.Name = "tpNetstat";
            this.tpNetstat.Padding = new System.Windows.Forms.Padding(3);
            this.tpNetstat.Size = new System.Drawing.Size(525, 393);
            this.tpNetstat.TabIndex = 1;
            this.tpNetstat.Text = "网络统计";
            this.tpNetstat.UseVisualStyleBackColor = true;
            // 
            // txtNetstat
            // 
            this.txtNetstat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNetstat.Location = new System.Drawing.Point(3, 3);
            this.txtNetstat.Multiline = true;
            this.txtNetstat.Name = "txtNetstat";
            this.txtNetstat.ReadOnly = true;
            this.txtNetstat.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNetstat.Size = new System.Drawing.Size(519, 387);
            this.txtNetstat.TabIndex = 0;
            // 
            // timerNetstat
            // 
            this.timerNetstat.Enabled = true;
            this.timerNetstat.Interval = 1000;
            this.timerNetstat.Tick += new System.EventHandler(this.timerNetstat_Tick);
            // 
            // ConnectionInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "ConnectionInfoControl";
            this.Size = new System.Drawing.Size(533, 426);
            this.tabControl1.ResumeLayout(false);
            this.tpNetstat.ResumeLayout(false);
            this.tpNetstat.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpBasic;
        private System.Windows.Forms.TabPage tpNetstat;
        private System.Windows.Forms.TextBox txtNetstat;
        private System.Windows.Forms.Timer timerNetstat;
    }
}
