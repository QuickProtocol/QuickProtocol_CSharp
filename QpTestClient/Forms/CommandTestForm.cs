using Quick.Protocol;
using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
            var commandRequestTypeName = txtCommandRequestTypeName.Text.Trim();
            if (string.IsNullOrEmpty(commandRequestTypeName))
            {
                txtCommandRequestTypeName.Focus();
                return;
            }
            var requestContent = txtTestRequest.Text.Trim();
            if (string.IsNullOrEmpty(requestContent))
            {
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
            txtTestRequest.Focus();
            txtTestResponse.Clear();
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
