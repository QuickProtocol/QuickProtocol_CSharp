
namespace QpTestClient.Controls
{
    partial class CommandInfoControl
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
            tpRequestSchemaSample = new System.Windows.Forms.TabPage();
            txtRequestSchemaSample = new System.Windows.Forms.TextBox();
            tpResponseSchemaSample = new System.Windows.Forms.TabPage();
            txtResponseSchemaSample = new System.Windows.Forms.TextBox();
            tabControl1.SuspendLayout();
            tpBasic.SuspendLayout();
            tpRequestSchemaSample.SuspendLayout();
            tpResponseSchemaSample.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tpBasic);
            tabControl1.Controls.Add(tpRequestSchemaSample);
            tabControl1.Controls.Add(tpResponseSchemaSample);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(672, 677);
            tabControl1.TabIndex = 1;
            // 
            // tpBasic
            // 
            tpBasic.Controls.Add(txtBasic);
            tpBasic.Location = new System.Drawing.Point(8, 45);
            tpBasic.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpBasic.Name = "tpBasic";
            tpBasic.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpBasic.Size = new System.Drawing.Size(656, 624);
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
            txtBasic.Size = new System.Drawing.Size(646, 614);
            txtBasic.TabIndex = 1;
            // 
            // tpRequestSchemaSample
            // 
            tpRequestSchemaSample.Controls.Add(txtRequestSchemaSample);
            tpRequestSchemaSample.Location = new System.Drawing.Point(8, 45);
            tpRequestSchemaSample.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpRequestSchemaSample.Name = "tpRequestSchemaSample";
            tpRequestSchemaSample.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpRequestSchemaSample.Size = new System.Drawing.Size(656, 624);
            tpRequestSchemaSample.TabIndex = 2;
            tpRequestSchemaSample.Text = "请求示例";
            tpRequestSchemaSample.UseVisualStyleBackColor = true;
            // 
            // txtRequestSchemaSample
            // 
            txtRequestSchemaSample.Dock = System.Windows.Forms.DockStyle.Fill;
            txtRequestSchemaSample.Location = new System.Drawing.Point(5, 5);
            txtRequestSchemaSample.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            txtRequestSchemaSample.Multiline = true;
            txtRequestSchemaSample.Name = "txtRequestSchemaSample";
            txtRequestSchemaSample.ReadOnly = true;
            txtRequestSchemaSample.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtRequestSchemaSample.Size = new System.Drawing.Size(646, 614);
            txtRequestSchemaSample.TabIndex = 1;
            // 
            // tpResponseSchemaSample
            // 
            tpResponseSchemaSample.Controls.Add(txtResponseSchemaSample);
            tpResponseSchemaSample.Location = new System.Drawing.Point(8, 45);
            tpResponseSchemaSample.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpResponseSchemaSample.Name = "tpResponseSchemaSample";
            tpResponseSchemaSample.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            tpResponseSchemaSample.Size = new System.Drawing.Size(656, 624);
            tpResponseSchemaSample.TabIndex = 4;
            tpResponseSchemaSample.Text = "响应示例";
            tpResponseSchemaSample.UseVisualStyleBackColor = true;
            // 
            // txtResponseSchemaSample
            // 
            txtResponseSchemaSample.Dock = System.Windows.Forms.DockStyle.Fill;
            txtResponseSchemaSample.Location = new System.Drawing.Point(5, 5);
            txtResponseSchemaSample.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            txtResponseSchemaSample.Multiline = true;
            txtResponseSchemaSample.Name = "txtResponseSchemaSample";
            txtResponseSchemaSample.ReadOnly = true;
            txtResponseSchemaSample.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtResponseSchemaSample.Size = new System.Drawing.Size(646, 614);
            txtResponseSchemaSample.TabIndex = 1;
            // 
            // CommandInfoControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl1);
            Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            Name = "CommandInfoControl";
            Size = new System.Drawing.Size(672, 677);
            tabControl1.ResumeLayout(false);
            tpBasic.ResumeLayout(false);
            tpBasic.PerformLayout();
            tpRequestSchemaSample.ResumeLayout(false);
            tpRequestSchemaSample.PerformLayout();
            tpResponseSchemaSample.ResumeLayout(false);
            tpResponseSchemaSample.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpBasic;
        private System.Windows.Forms.TextBox txtBasic;
        private System.Windows.Forms.TabPage tpRequestSchemaSample;
        private System.Windows.Forms.TextBox txtRequestSchemaSample;
        private System.Windows.Forms.TabPage tpResponseSchemaSample;
        private System.Windows.Forms.TextBox txtResponseSchemaSample;
    }
}
