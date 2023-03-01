namespace QpTestClient
{
    partial class QuickConnectForm
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
            txtUrl = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            btnOk = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            textBox1 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // txtUrl
            // 
            txtUrl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtUrl.Location = new System.Drawing.Point(118, 58);
            txtUrl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new System.Drawing.Size(502, 30);
            txtUrl.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(57, 62);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(48, 24);
            label2.TabIndex = 5;
            label2.Text = "URL:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(55, 316);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 24);
            label1.TabIndex = 5;
            label1.Text = "密码:";
            // 
            // txtPassword
            // 
            txtPassword.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtPassword.Location = new System.Drawing.Point(116, 312);
            txtPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new System.Drawing.Size(502, 30);
            txtPassword.TabIndex = 5;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(237, 353);
            btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(115, 36);
            btnCancel.TabIndex = 101;
            btnCancel.Text = "取消(&C)";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOk
            // 
            btnOk.Location = new System.Drawing.Point(116, 353);
            btnOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(115, 36);
            btnOk.TabIndex = 100;
            btnOk.Text = "确定(&O)";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(57, 21);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(50, 24);
            label4.TabIndex = 5;
            label4.Text = "名称:";
            // 
            // txtName
            // 
            txtName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtName.Location = new System.Drawing.Point(118, 17);
            txtName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(502, 30);
            txtName.TabIndex = 3;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox1.Location = new System.Drawing.Point(118, 99);
            textBox1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new System.Drawing.Size(501, 203);
            textBox1.TabIndex = 102;
            textBox1.Text = "TCP: qp.tcp://127.0.0.1:3000\r\nUDP: qp.udp://127.0.0.1:3000\r\nWebSocket: qp.ws://127.0.0.1:3000\r\n命名管道: qp.pipe://./Quick.Protocol\r\n串口: qp.serial://COM1";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(58, 103);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(50, 24);
            label3.TabIndex = 103;
            label3.Text = "示例:";
            // 
            // QuickConnectForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(640, 415);
            Controls.Add(label3);
            Controls.Add(textBox1);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(txtPassword);
            Controls.Add(label1);
            Controls.Add(txtName);
            Controls.Add(label4);
            Controls.Add(txtUrl);
            Controls.Add(label2);
            Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            Name = "QuickConnectForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "快速添加连接";
            Load += QuickConnectForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
    }
}