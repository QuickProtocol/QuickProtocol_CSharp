using Quick.Protocol.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

namespace QpTestClient
{
    public partial class QuickConnectForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TestConnectionInfo ConnectionInfo { get; private set; }

        public QuickConnectForm()
        {
            InitializeComponent();
            var currentAssembly = this.GetType().Assembly;
            //窗体图标
            using (var stream = currentAssembly.GetManifestResourceStream($"{nameof(QpTestClient)}.Images.connection.ico"))
                Icon = new Icon(stream);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("请输入名称！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入URL！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return;
            }

            var password = txtPassword.Text.Trim();
            if (pnlPassword.Visible)
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("请输入密码！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }
            }
            var uri = new Uri(url);
            Quick.Protocol.QpClientOptions options = null;
            try
            {
                options = Quick.Protocol.QpClientOptions.Parse(uri);
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析URL时出错，原因：" + ExceptionUtils.GetExceptionString(ex), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (pnlPassword.Visible)
                options.Password = password;
            ConnectionInfo = new TestConnectionInfo()
            {
                Name = name,
                QpClientTypeName = options.CreateClient().GetType().FullName,
                QpClientOptions = options
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void QuickConnectForm_Load(object sender, EventArgs e)
        {
            txtName.Text = "快速添加连接_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {
            var url = txtUrl.Text.Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                pnlPassword.Visible = true;
                return;
            }
            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
            pnlPassword.Visible = string.IsNullOrEmpty(queryString.Get("Password"));
        }
    }
}
