using System.Text.Json;
using Quick.Protocol;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient
{
    public partial class ConnectForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TestConnectionInfo ConnectionInfo { get; private set; }
        private QpClientOptions clientOptions = null;
        public ConnectForm()
        {
            InitializeComponent();
            var currentAssembly = this.GetType().Assembly;
            //窗体图标
            using (var stream = currentAssembly.GetManifestResourceStream($"{nameof(QpTestClient)}.Images.connection.ico"))
                Icon = new Icon(stream);
        }

        public void EditConnectionInfo(TestConnectionInfo connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
            txtName.Text = connectionInfo.Name;
            var qpClientTypeInfo = QpClientTypeManager.Instance.GetAll().FirstOrDefault(t => t.ClientType.FullName == connectionInfo.QpClientTypeName);
            cbConnectType.SelectedItem = qpClientTypeInfo;
            Text = "编辑连接";
        }

        private void ConnectForm_Load(object sender, EventArgs e)
        {
            foreach (var info in QpClientTypeManager.Instance.GetAll())
                cbConnectType.Items.Add(info);

            if (cbConnectType.Items.Count <= 0)
                return;

            if (ConnectionInfo != null)
            {
                var qpClientTypeName = ConnectionInfo.QpClientTypeName;
                var item = QpClientTypeManager.Instance.GetAll().FirstOrDefault(t => t.ClientType.FullName == qpClientTypeName);
                cbConnectType.SelectedItem = item;
            }
            else
            {
                cbConnectType.SelectedIndex = 0;
            }
        }

        private void cbConnectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var qpClientTypeInfo = (QpClientTypeInfo)cbConnectType.SelectedItem;
            if (ConnectionInfo != null && qpClientTypeInfo.ClientType.FullName == ConnectionInfo.QpClientTypeName)
            {
                clientOptions = ConnectionInfo.QpClientOptions.Clone();
            }
            else
            {
                clientOptions = qpClientTypeInfo.CreateOptionsInstanceFunc();
            }
            pnlClientOptions.Controls.Clear();
            var control = qpClientTypeInfo.CreateOptionsControlFunc();
            control.DataContext = clientOptions;
            control.Dock = DockStyle.Fill;
            pnlClientOptions.Controls.Add(control);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var qpClientTypeInfo = (QpClientTypeInfo)cbConnectType.SelectedItem;
            var name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("请输入连接名称！", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            ConnectionInfo = new TestConnectionInfo()
            {
                Name = name,
                QpClientTypeName = qpClientTypeInfo.ClientType.FullName,
                QpClientOptions = clientOptions,
                Instructions = ConnectionInfo?.Instructions
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
