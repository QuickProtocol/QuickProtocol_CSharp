
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpBasic = new System.Windows.Forms.TabPage();
            this.txtBasic = new System.Windows.Forms.TextBox();
            this.tpSchema = new System.Windows.Forms.TabPage();
            this.txtSchema = new System.Windows.Forms.TextBox();
            this.tpSchemaSample = new System.Windows.Forms.TabPage();
            this.txtSchemaSample = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tpBasic.SuspendLayout();
            this.tpSchema.SuspendLayout();
            this.tpSchemaSample.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpBasic);
            this.tabControl1.Controls.Add(this.tpSchema);
            this.tabControl1.Controls.Add(this.tpSchemaSample);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(533, 426);
            this.tabControl1.TabIndex = 0;
            // 
            // tpBasic
            // 
            this.tpBasic.Controls.Add(this.txtBasic);
            this.tpBasic.Location = new System.Drawing.Point(4, 29);
            this.tpBasic.Name = "tpBasic";
            this.tpBasic.Padding = new System.Windows.Forms.Padding(3);
            this.tpBasic.Size = new System.Drawing.Size(525, 393);
            this.tpBasic.TabIndex = 0;
            this.tpBasic.Text = "基本";
            this.tpBasic.UseVisualStyleBackColor = true;
            // 
            // txtBasic
            // 
            this.txtBasic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBasic.Location = new System.Drawing.Point(3, 3);
            this.txtBasic.Multiline = true;
            this.txtBasic.Name = "txtBasic";
            this.txtBasic.ReadOnly = true;
            this.txtBasic.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBasic.Size = new System.Drawing.Size(519, 387);
            this.txtBasic.TabIndex = 1;
            // 
            // tpSchema
            // 
            this.tpSchema.Controls.Add(this.txtSchema);
            this.tpSchema.Location = new System.Drawing.Point(4, 29);
            this.tpSchema.Name = "tpSchema";
            this.tpSchema.Padding = new System.Windows.Forms.Padding(3);
            this.tpSchema.Size = new System.Drawing.Size(525, 393);
            this.tpSchema.TabIndex = 1;
            this.tpSchema.Text = "定义";
            this.tpSchema.UseVisualStyleBackColor = true;
            // 
            // txtSchema
            // 
            this.txtSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSchema.Location = new System.Drawing.Point(3, 3);
            this.txtSchema.Multiline = true;
            this.txtSchema.Name = "txtSchema";
            this.txtSchema.ReadOnly = true;
            this.txtSchema.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSchema.Size = new System.Drawing.Size(519, 387);
            this.txtSchema.TabIndex = 0;
            // 
            // tpSchemaSample
            // 
            this.tpSchemaSample.Controls.Add(this.txtSchemaSample);
            this.tpSchemaSample.Location = new System.Drawing.Point(4, 29);
            this.tpSchemaSample.Name = "tpSchemaSample";
            this.tpSchemaSample.Padding = new System.Windows.Forms.Padding(3);
            this.tpSchemaSample.Size = new System.Drawing.Size(525, 393);
            this.tpSchemaSample.TabIndex = 2;
            this.tpSchemaSample.Text = "示例";
            this.tpSchemaSample.UseVisualStyleBackColor = true;
            // 
            // txtSchemaSample
            // 
            this.txtSchemaSample.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSchemaSample.Location = new System.Drawing.Point(3, 3);
            this.txtSchemaSample.Multiline = true;
            this.txtSchemaSample.Name = "txtSchemaSample";
            this.txtSchemaSample.ReadOnly = true;
            this.txtSchemaSample.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSchemaSample.Size = new System.Drawing.Size(519, 387);
            this.txtSchemaSample.TabIndex = 1;
            // 
            // NoticeInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "NoticeInfoControl";
            this.Size = new System.Drawing.Size(533, 426);
            this.tabControl1.ResumeLayout(false);
            this.tpBasic.ResumeLayout(false);
            this.tpBasic.PerformLayout();
            this.tpSchema.ResumeLayout(false);
            this.tpSchema.PerformLayout();
            this.tpSchemaSample.ResumeLayout(false);
            this.tpSchemaSample.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpBasic;
        private System.Windows.Forms.TabPage tpSchema;
        private System.Windows.Forms.TextBox txtSchema;
        private System.Windows.Forms.TabPage tpSchemaSample;
        private System.Windows.Forms.TextBox txtSchemaSample;
        private System.Windows.Forms.TextBox txtBasic;
    }
}
