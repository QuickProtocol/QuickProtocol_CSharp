using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient
{
    public partial class QuickConnectForm : Form
    {
        public TestConnectionInfo ConnectionInfo { get; private set; }

        public QuickConnectForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入URL！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return;
            }
            var password = txtPassword.Text.Trim();
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入密码！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }
            var uri = new Uri(url);
            var connectionInfo = Quick.Protocol.AllClients.ConnectionUriParser.Parse(uri);
            if (connectionInfo==null)
            {
                MessageBox.Show("无效的URL！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ConnectionInfo = new TestConnectionInfo()
            {
                Name = "快速添加连接_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                QpClientTypeName = connectionInfo.QpClientType.FullName,
                QpClientOptions = connectionInfo.QpClientOptions
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
