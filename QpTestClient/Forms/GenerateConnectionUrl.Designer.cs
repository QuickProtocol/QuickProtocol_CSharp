namespace QpTestClient.Forms
{
    partial class GenerateConnectionUrl
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
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.cbIncludePassword = new System.Windows.Forms.CheckBox();
            this.cbIncludeOtherProperty = new System.Windows.Forms.CheckBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.Location = new System.Drawing.Point(13, 72);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(442, 313);
            this.txtOutput.TabIndex = 1;
            // 
            // cbIncludePassword
            // 
            this.cbIncludePassword.AutoSize = true;
            this.cbIncludePassword.Location = new System.Drawing.Point(13, 12);
            this.cbIncludePassword.Name = "cbIncludePassword";
            this.cbIncludePassword.Size = new System.Drawing.Size(91, 24);
            this.cbIncludePassword.TabIndex = 2;
            this.cbIncludePassword.Text = "包含密码";
            this.cbIncludePassword.UseVisualStyleBackColor = true;
            this.cbIncludePassword.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // cbIncludeOtherProperty
            // 
            this.cbIncludeOtherProperty.AutoSize = true;
            this.cbIncludeOtherProperty.Location = new System.Drawing.Point(13, 42);
            this.cbIncludeOtherProperty.Name = "cbIncludeOtherProperty";
            this.cbIncludeOtherProperty.Size = new System.Drawing.Size(121, 24);
            this.cbIncludeOtherProperty.TabIndex = 2;
            this.cbIncludeOtherProperty.Text = "包含其他属性";
            this.cbIncludeOtherProperty.UseVisualStyleBackColor = true;
            this.cbIncludeOtherProperty.CheckedChanged += new System.EventHandler(this.Checkbox_CheckedChanged);
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopy.Location = new System.Drawing.Point(13, 393);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(94, 29);
            this.btnCopy.TabIndex = 3;
            this.btnCopy.Text = "复制";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // GenerateConnectionUrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 434);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.cbIncludePassword);
            this.Controls.Add(this.cbIncludeOtherProperty);
            this.Controls.Add(this.txtOutput);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateConnectionUrl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GenerateConnectionUrl";
            this.Load += new System.EventHandler(this.GenerateConnectionUrl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.CheckBox cbIncludePassword;
        private System.Windows.Forms.CheckBox cbIncludeOtherProperty;
        private System.Windows.Forms.Button btnCopy;
    }
}