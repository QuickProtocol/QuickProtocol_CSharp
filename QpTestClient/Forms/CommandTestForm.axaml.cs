using Avalonia.Controls;
using Avalonia.Interactivity;
using Quick.Protocol;
using Quick.Utils;
using System;

namespace QpTestClient.Forms
{
    public partial class CommandTestForm : Window
    {
        private readonly ConnectionContext connectionContext;

        public CommandTestForm() { }
        public CommandTestForm(ConnectionContext connectionContext, QpCommandInfo qpCommandInfo = null)
        {
            this.connectionContext = connectionContext;
            InitializeComponent();

            if (qpCommandInfo == null)
            {
                TxtFormTitle.Text = $"{Title} - {connectionContext.ConnectionInfo.Name}";
            }
            else
            {
                TxtFormTitle.Text = $"{Title} - {qpCommandInfo.Name} - {connectionContext.ConnectionInfo.Name}";
                TxtTestRequest.Text = qpCommandInfo.RequestTypeSchemaSample;
                TxtCommandRequestTypeName.Text = qpCommandInfo.RequestTypeName;
            }
        }

        private void TxtFormTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            Title = TxtFormTitle.Text?.Trim() ?? "命令测试";
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            TxtTestResponse.Clear();

            var commandRequestTypeName = TxtCommandRequestTypeName.Text?.Trim();
            if (string.IsNullOrEmpty(commandRequestTypeName))
            {
                TxtTestResponse.Text = $"{DateTime.Now.ToLongTimeString()}: 请输入命令请求类型！";
                TxtCommandRequestTypeName.Focus();
                return;
            }

            var requestContent = TxtTestRequest.Text?.Trim();
            if (string.IsNullOrEmpty(requestContent))
            {
                TxtTestResponse.Text = $"{DateTime.Now.ToLongTimeString()}: 请输入请求内容！";
                TxtTestRequest.Focus();
                return;
            }

            var qpClient = connectionContext.QpClient;
            if (qpClient == null)
            {
                TxtTestResponse.Text = $"{DateTime.Now.ToLongTimeString()}: 当前未连接，无法执行！{Environment.NewLine}";
                return;
            }

            BtnSend.IsEnabled = false;
            TxtTestResponse.Text = $"{DateTime.Now.ToLongTimeString()}: 开始执行...{Environment.NewLine}";
            try
            {
                var ret = await qpClient.SendCommand(commandRequestTypeName, requestContent);
                TxtTestResponse.Text += $"{DateTime.Now.ToLongTimeString()}: 执行成功{Environment.NewLine}";
                TxtTestResponse.Text += $"命令响应类型：{ret.TypeName}{Environment.NewLine}";
                TxtTestResponse.Text += $"响应内容{Environment.NewLine}";
                TxtTestResponse.Text += $"--------------------------{Environment.NewLine}";
                TxtTestResponse.Text += ret.Content;
            }
            catch (Exception ex)
            {
                TxtTestResponse.Text += $"{DateTime.Now.ToLongTimeString()}: 执行失败{Environment.NewLine}";
                TxtTestResponse.Text += $"错误信息{Environment.NewLine}";
                TxtTestResponse.Text += $"--------------------------{Environment.NewLine}";
                TxtTestResponse.Text += ExceptionUtils.GetExceptionMessage(ex);
            }
            BtnSend.IsEnabled = true;
        }
    }
}
