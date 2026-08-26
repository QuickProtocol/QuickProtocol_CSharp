using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Quick.Protocol;
using System;

namespace QpTestClient.Controls
{
    public partial class ConnectionInfoControl : UserControl
    {
        private readonly ConnectionContext item;
        private string lastNetstatStr = string.Empty;
        private readonly DispatcherTimer timer;

        public ConnectionInfoControl(ConnectionContext item)
        {
            this.item = item;
            InitializeComponent();

            var qpClientTypeInfo = QpClientTypeManager.Instance.Get(item.ConnectionInfo.QpClientTypeName);
            if (qpClientTypeInfo != null)
            {
                var ctl = new AotPropertyGrid();
                qpClientTypeInfo.EditOptions(ctl, item.ConnectionInfo.QpClientOptions);
                ctl.GenerateControls();
                ctl.ReadOnly = true;
                TpBasic.Child = ctl;
            }

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            QpChannel? channel = item.QpClient;
            if (!item.Connected || channel == null)
            {
                ShowNetStat("当前未连接");
                return;
            }
            if (!item.ConnectionInfo.QpClientOptions.EnableNetstat)
            {
                ShowNetStat("当前连接没有配置启用网络统计功能");
                return;
            }
            ShowNetStat($@"发送的字节数：{channel.BytesSent:N0}
接收的字节数：{channel.BytesReceived:N0}
每秒发送字节数：{channel.BytesSentPerSec:N0}
每秒接收字节数：{channel.BytesReceivedPerSec:N0}");
        }

        private void ShowNetStat(string content)
        {
            if (content == lastNetstatStr)
                return;
            lastNetstatStr = content;
            TxtNetstat.Text = content;
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            timer.Start();
        }
    }
}
