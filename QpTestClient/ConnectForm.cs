using System.Text.Json;
using Quick.Protocol;
using Quick.Protocol.Utils;
using Quick.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient
{
    public partial class ConnectForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TestConnectionInfo ConnectionInfo { get; private set; }

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
            this.ConnectionInfo = XmlConvert.Deserialize<TestConnectionInfo>(XmlConvert.Serialize(connectionInfo));
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
            var qpClientTypeName = "Quick.Protocol.Tcp.QpTcpClient";
            if (ConnectionInfo != null)
                qpClientTypeName = ConnectionInfo.QpClientTypeName;
            var item = QpClientTypeManager.Instance.GetAll().FirstOrDefault(t => t.ClientType.FullName == qpClientTypeName);
            if (item != null)
                cbConnectType.SelectedItem = item;
            else
                cbConnectType.SelectedIndex = 0;
        }

        private void cbConnectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var qpClientTypeInfo = (QpClientTypeInfo)cbConnectType.SelectedItem;

            QpClientOptions options = null;
            if (ConnectionInfo != null && qpClientTypeInfo.ClientType.FullName == ConnectionInfo.QpClientTypeName)
            {
                options = (QpClientOptions)JsonSerializer.Deserialize(
                    JsonSerializer.Serialize(ConnectionInfo.QpClientOptions),
                    qpClientTypeInfo.OptionsType);
            }
            else
            {
                options = qpClientTypeInfo.CreateOptionsInstanceFunc();
            }
            pgOptions.SelectedObject = options;
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
                QpClientOptions = (QpClientOptions)pgOptions.SelectedObject,
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
