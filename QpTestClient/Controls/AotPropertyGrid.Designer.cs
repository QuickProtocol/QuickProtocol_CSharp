namespace QpTestClient.Controls
{
    partial class AotPropertyGrid
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
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            flp = new System.Windows.Forms.FlowLayoutPanel();
            lblPropertyDescription = new System.Windows.Forms.Label();
            lblPropertyName = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(flp);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(panel1);
            splitContainer1.Size = new System.Drawing.Size(845, 921);
            splitContainer1.SplitterDistance = 718;
            splitContainer1.TabIndex = 0;
            // 
            // flp
            // 
            flp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flp.Dock = System.Windows.Forms.DockStyle.Fill;
            flp.Location = new System.Drawing.Point(0, 0);
            flp.Name = "flp";
            flp.Size = new System.Drawing.Size(845, 718);
            flp.TabIndex = 0;
            // 
            // lblPropertyDescription
            // 
            lblPropertyDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            lblPropertyDescription.Location = new System.Drawing.Point(0, 31);
            lblPropertyDescription.Name = "lblPropertyDescription";
            lblPropertyDescription.Size = new System.Drawing.Size(843, 166);
            lblPropertyDescription.TabIndex = 1;
            // 
            // lblPropertyName
            // 
            lblPropertyName.Dock = System.Windows.Forms.DockStyle.Top;
            lblPropertyName.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            lblPropertyName.Location = new System.Drawing.Point(0, 0);
            lblPropertyName.Name = "lblPropertyName";
            lblPropertyName.Size = new System.Drawing.Size(843, 31);
            lblPropertyName.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(lblPropertyDescription);
            panel1.Controls.Add(lblPropertyName);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(845, 199);
            panel1.TabIndex = 2;
            // 
            // AotPropertyGrid
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "AotPropertyGrid";
            Size = new System.Drawing.Size(845, 921);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flp;
        private System.Windows.Forms.Label lblPropertyDescription;
        private System.Windows.Forms.Label lblPropertyName;
        private System.Windows.Forms.Panel panel1;
    }
}
