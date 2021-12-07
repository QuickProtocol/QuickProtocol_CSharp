
namespace QpTestClient.Forms
{
    partial class CommandTestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandTestForm));
            this.scTest = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtTestRequest = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtTestResponse = new System.Windows.Forms.TextBox();
            this.txtCommandRequestTypeName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFormTitle = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.scTest)).BeginInit();
            this.scTest.Panel1.SuspendLayout();
            this.scTest.Panel2.SuspendLayout();
            this.scTest.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // scTest
            // 
            this.scTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scTest.Location = new System.Drawing.Point(12, 113);
            this.scTest.Name = "scTest";
            this.scTest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scTest.Panel1
            // 
            this.scTest.Panel1.Controls.Add(this.groupBox1);
            // 
            // scTest.Panel2
            // 
            this.scTest.Panel2.Controls.Add(this.groupBox2);
            this.scTest.Size = new System.Drawing.Size(620, 465);
            this.scTest.SplitterDistance = 224;
            this.scTest.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtTestRequest);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(620, 224);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "请求内容";
            // 
            // txtTestRequest
            // 
            this.txtTestRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTestRequest.Location = new System.Drawing.Point(3, 23);
            this.txtTestRequest.Multiline = true;
            this.txtTestRequest.Name = "txtTestRequest";
            this.txtTestRequest.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTestRequest.Size = new System.Drawing.Size(614, 198);
            this.txtTestRequest.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtTestResponse);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(620, 237);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "结果";
            // 
            // txtTestResponse
            // 
            this.txtTestResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTestResponse.Location = new System.Drawing.Point(3, 23);
            this.txtTestResponse.Multiline = true;
            this.txtTestResponse.Name = "txtTestResponse";
            this.txtTestResponse.ReadOnly = true;
            this.txtTestResponse.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTestResponse.Size = new System.Drawing.Size(614, 211);
            this.txtTestResponse.TabIndex = 2;
            // 
            // txtCommandRequestTypeName
            // 
            this.txtCommandRequestTypeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommandRequestTypeName.Location = new System.Drawing.Point(112, 45);
            this.txtCommandRequestTypeName.Name = "txtCommandRequestTypeName";
            this.txtCommandRequestTypeName.Size = new System.Drawing.Size(520, 27);
            this.txtCommandRequestTypeName.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "命令请求类型:";
            // 
            // txtFormTitle
            // 
            this.txtFormTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFormTitle.Location = new System.Drawing.Point(112, 12);
            this.txtFormTitle.Name = "txtFormTitle";
            this.txtFormTitle.Size = new System.Drawing.Size(520, 27);
            this.txtFormTitle.TabIndex = 10;
            this.txtFormTitle.TextChanged += new System.EventHandler(this.txtFormTitle_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "窗口标题:";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(112, 78);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(94, 29);
            this.btnSend.TabIndex = 8;
            this.btnSend.Text = "发送";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // CommandTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 590);
            this.Controls.Add(this.txtCommandRequestTypeName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFormTitle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.scTest);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CommandTestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "命令测试";
            this.scTest.Panel1.ResumeLayout(false);
            this.scTest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scTest)).EndInit();
            this.scTest.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer scTest;
        private System.Windows.Forms.TextBox txtTestRequest;
        private System.Windows.Forms.TextBox txtTestResponse;
        private System.Windows.Forms.TextBox txtCommandRequestTypeName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFormTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}