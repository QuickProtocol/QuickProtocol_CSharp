using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient.Forms
{
    public partial class GenerateConnectionUrl : Form
    {
        private ConnectionContext connectionContext;
        public GenerateConnectionUrl(ConnectionContext connectionContext)
        {
            this.connectionContext=connectionContext;
            InitializeComponent();
        }

        private void GenerateConnectionUrl_Load(object sender, EventArgs e)
        {
            Text =$"生成[{connectionContext.ConnectionInfo.Name}]连接URL";
            generate();
        }

        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            generate();
        }

        private void generate()
        {
            var includePassword = cbIncludePassword.Checked;
            var includeOtherProperty = cbIncludeOtherProperty.Checked;

            txtOutput.Text = connectionContext.ConnectionInfo.QpClientOptions.ToUri(
                includePassword,
                includeOtherProperty
                ).ToString();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtOutput.Text);
            MessageBox.Show("已复制到剪贴板！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
