using Quick.Protocol;
using Quick.Protocol.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QpTestClient.Forms
{
    public partial class CommandTestForm : Form
    {
        private ConnectionContext connectionContext;

        public CommandTestForm(ConnectionContext connectionContext, QpCommandInfo qpCommandInfo = null)
        {
            this.connectionContext = connectionContext;

            InitializeComponent();
            var currentAssembly = this.GetType().Assembly;
            //窗体图标
            using (var stream = currentAssembly.GetManifestResourceStream($"{nameof(QpTestClient)}.Images.connection.ico"))
                Icon = new Icon(stream);
            if (qpCommandInfo == null)
            {
                txtFormTitle.Text = $"{Text} - {connectionContext.ConnectionInfo.Name}";
            }
            else
            {
                txtFormTitle.Text = $"{Text} - {qpCommandInfo.Name} - {connectionContext.ConnectionInfo.Name}";
                txtTestRequest.Text = qpCommandInfo.RequestTypeSchemaSample;
                txtCommandRequestTypeName.Text = qpCommandInfo.RequestTypeName;
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            txtTestRequest.Focus();
            txtTestResponse.Clear();

            var commandRequestTypeName = txtCommandRequestTypeName.Text.Trim();
            if (string.IsNullOrEmpty(commandRequestTypeName))
            {
                txtTestResponse.AppendText($"{DateTime.Now.ToLongTimeString()}: 请输入命令请求类型！");
                txtCommandRequestTypeName.Focus();                
                return;
            }
            var requestContent = txtTestRequest.Text.Trim();
            if (string.IsNullOrEmpty(requestContent))
            {
                txtTestResponse.AppendText($"{DateTime.Now.ToLongTimeString()}: 请输入请求内容！");
                txtTestRequest.Focus();
                return;
            }

            var qpClient = connectionContext.QpClient;
            if (qpClient == null)
            {
                txtTestResponse.AppendText($"{DateTime.Now.ToLongTimeString()}: 当前未连接，无法执行！{Environment.NewLine}");
                return;
            }

            btnSend.Enabled = false;
            txtTestResponse.AppendText($"{DateTime.Now.ToLongTimeString()}: 开始执行...{Environment.NewLine}");
            try
            {
                var ret = await qpClient.SendCommand(commandRequestTypeName, requestContent);
                if (txtTestResponse.IsDisposed)
                    return;
                txtTestResponse.AppendText($"{DateTime.Now.ToLongTimeString()}: 执行成功{Environment.NewLine}");
                txtTestResponse.AppendText($"命令响应类型：{ret.TypeName}{Environment.NewLine}");
                txtTestResponse.AppendText($"响应内容{Environment.NewLine}");
                txtTestResponse.AppendText($"--------------------------{Environment.NewLine}");
                txtTestResponse.AppendText(ret.Content);
            }
            catch (Exception ex)
            {
                if (txtTestResponse.IsDisposed)
                    return;
                txtTestResponse.AppendText($"{DateTime.Now.ToLongTimeString()}: 执行失败{Environment.NewLine}");
                txtTestResponse.AppendText($"错误信息{Environment.NewLine}");
                txtTestResponse.AppendText($"--------------------------{Environment.NewLine}");
                txtTestResponse.AppendText(ExceptionUtils.GetExceptionMessage(ex));
            }
            btnSend.Enabled = true;
        }

        private void txtFormTitle_TextChanged(object sender, EventArgs e)
        {
            Text = txtFormTitle.Text.Trim();
        }
    }
}
