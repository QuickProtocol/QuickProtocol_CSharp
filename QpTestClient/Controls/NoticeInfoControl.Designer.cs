
namespace QpTestClient.Controls
{
    partial class NoticeInfoControl
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
            tabControl1 = new System.Windows.Forms.TabControl();
            tpBasic = new System.Windows.Forms.TabPage();
            txtBasic = new System.Windows.Forms.TextBox();
            tpSchemaSample = new System.Windows.Forms.TabPage();
            txtSchemaSample = new System.Windows.Forms.TextBox();
            tabControl1.SuspendLayout();
            tpBasic.SuspendLayout();
            tpSchemaSample.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tpBasic);
            tabControl1.Controls.Add(tpSchemaSample);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(829, 660);
            tabControl1.TabIndex = 0;
            // 
            // tpBasic
            // 
            tpBasic.Controls.Add(txtBasic);
            tpBasic.Location = new System.Drawing.Point(8, 45);
            tpBasic.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpBasic.Name = "tpBasic";
            tpBasic.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpBasic.Size = new System.Drawing.Size(813, 607);
            tpBasic.TabIndex = 0;
            tpBasic.Text = "基本";
            tpBasic.UseVisualStyleBackColor = true;
            // 
            // txtBasic
            // 
            txtBasic.Dock = System.Windows.Forms.DockStyle.Fill;
            txtBasic.Location = new System.Drawing.Point(5, 5);
            txtBasic.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            txtBasic.Multiline = true;
            txtBasic.Name = "txtBasic";
            txtBasic.ReadOnly = true;
            txtBasic.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtBasic.Size = new System.Drawing.Size(803, 597);
            txtBasic.TabIndex = 1;
            // 
            // tpSchemaSample
            // 
            tpSchemaSample.Controls.Add(txtSchemaSample);
            tpSchemaSample.Location = new System.Drawing.Point(8, 45);
            tpSchemaSample.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpSchemaSample.Name = "tpSchemaSample";
            tpSchemaSample.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpSchemaSample.Size = new System.Drawing.Size(813, 607);
            tpSchemaSample.TabIndex = 2;
            tpSchemaSample.Text = "示例";
            tpSchemaSample.UseVisualStyleBackColor = true;
            // 
            // txtSchemaSample
            // 
            txtSchemaSample.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSchemaSample.Location = new System.Drawing.Point(5, 5);
            txtSchemaSample.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            txtSchemaSample.Multiline = true;
            txtSchemaSample.Name = "txtSchemaSample";
            txtSchemaSample.ReadOnly = true;
            txtSchemaSample.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtSchemaSample.Size = new System.Drawing.Size(803, 597);
            txtSchemaSample.TabIndex = 1;
            // 
            // NoticeInfoControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl1);
            Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Name = "NoticeInfoControl";
            Size = new System.Drawing.Size(829, 660);
            tabControl1.ResumeLayout(false);
            tpBasic.ResumeLayout(false);
            tpBasic.PerformLayout();
            tpSchemaSample.ResumeLayout(false);
            tpSchemaSample.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpBasic;
        private System.Windows.Forms.TabPage tpSchemaSample;
        private System.Windows.Forms.TextBox txtSchemaSample;
        private System.Windows.Forms.TextBox txtBasic;
    }
}
