using Newtonsoft.Json;
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
        public ConnectionInfo ConnectionInfo { get; private set; }

        public ConnectForm()
        {
            InitializeComponent();
        }

        public void EditConnectionInfo(ConnectionInfo connectionInfo)
        {            
            this.ConnectionInfo = XmlConvert.Deserialize<ConnectionInfo>(XmlConvert.Serialize(connectionInfo));
            txtName.Text = connectionInfo.Name;
            var qpClientTypeInfo = QpClientTypeManager.Instance.GetAll().FirstOrDefault(t => t.QpClientType.FullName == connectionInfo.QpClientTypeName);
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
            var item = QpClientTypeManager.Instance.GetAll().FirstOrDefault(t => t.QpClientType.FullName == qpClientTypeName);
            if (item != null)
                cbConnectType.SelectedItem = item;
            else
                cbConnectType.SelectedIndex = 0;
        }

        private void cbConnectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var qpClientTypeInfo = (QpClientTypeInfo)cbConnectType.SelectedItem;

            QpClientOptions options = null;
            if (ConnectionInfo != null && qpClientTypeInfo.QpClientType.FullName == ConnectionInfo.QpClientTypeName)
            {
                options = (QpClientOptions)JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(ConnectionInfo.QpClientOptions),
                    qpClientTypeInfo.QpClientOptionsType);
            }
            else
            {
                options = (QpClientOptions)Activator.CreateInstance(qpClientTypeInfo.QpClientOptionsType);
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
            ConnectionInfo = new ConnectionInfo()
            {
                Name = name,
                QpClientTypeName = qpClientTypeInfo.QpClientType.FullName,
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
