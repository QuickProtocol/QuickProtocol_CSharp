
using System.IO;

namespace QpTestClient
{
    partial class ConnectForm
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
            cbConnectType = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            pnlClientOptions = new System.Windows.Forms.Panel();
            SuspendLayout();
            // 
            // cbConnectType
            // 
            cbConnectType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbConnectType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbConnectType.FormattingEnabled = true;
            cbConnectType.Location = new System.Drawing.Point(157, 79);
            cbConnectType.Margin = new System.Windows.Forms.Padding(5);
            cbConnectType.Name = "cbConnectType";
            cbConnectType.Size = new System.Drawing.Size(636, 39);
            cbConnectType.TabIndex = 2;
            cbConnectType.SelectedIndexChanged += cbConnectType_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(34, 84);
            label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(116, 31);
            label1.TabIndex = 3;
            label1.Text = "连接方式:";
            // 
            // txtName
            // 
            txtName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtName.Location = new System.Drawing.Point(157, 19);
            txtName.Margin = new System.Windows.Forms.Padding(5);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(636, 38);
            txtName.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(81, 23);
            label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(68, 31);
            label2.TabIndex = 3;
            label2.Text = "名称:";
            // 
            // btnOk
            // 
            btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnOk.Location = new System.Drawing.Point(157, 756);
            btnOk.Margin = new System.Windows.Forms.Padding(5);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(146, 45);
            btnOk.TabIndex = 5;
            btnOk.Text = "确定(&O)";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnCancel.Location = new System.Drawing.Point(313, 756);
            btnCancel.Margin = new System.Windows.Forms.Padding(5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(146, 45);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "取消(&C)";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // pnlClientOptions
            // 
            pnlClientOptions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlClientOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlClientOptions.Location = new System.Drawing.Point(34, 126);
            pnlClientOptions.Name = "pnlClientOptions";
            pnlClientOptions.Size = new System.Drawing.Size(759, 624);
            pnlClientOptions.TabIndex = 7;
            // 
            // ConnectForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(814, 815);
            Controls.Add(pnlClientOptions);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(txtName);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cbConnectType);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "添加连接";
            Load += ConnectForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ComboBox cbConnectType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlClientOptions;
    }
}