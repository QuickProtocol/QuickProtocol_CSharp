using Quick.Protocol;
using System;
using System.Windows.Forms;

namespace QpTestClient.Controls
{
    public partial class ConnectionInfoControl : UserControl
    {
        private ConnectionContext item;
        public ConnectionInfoControl(ConnectionContext item)
        {
            this.item = item;
            InitializeComponent();
            var qpClientTypeInfo = QpClientTypeManager.Instance.Get(item.ConnectionInfo.QpClientTypeName);
            var ctl = new AotPropertyGrid();
            qpClientTypeInfo.EditOptions(ctl, item.ConnectionInfo.QpClientOptions);
            ctl.GenerateControls();
            ctl.ReadOnly = true;
            ctl.Dock = DockStyle.Fill;
            tpBasic.Controls.Add(ctl);
        }
        private string lastNetstatStr = string.Empty;
        private void timerNetstat_Tick(object sender, EventArgs e)
        {
            QpChannel channel = item.QpClient;
            if (!item.Connected || channel == null)
            {
                showNetStat("当前未连接");
                return;
            }
            if (!item.ConnectionInfo.QpClientOptions.EnableNetstat)
            {
                showNetStat("当前连接没有配置启用网络统计功能");
                return;
            }
            showNetStat(@$"发送的字节数：{channel.BytesSent:N0}
接收的字节数：{channel.BytesReceived:N0}
每秒发送字节数：{channel.BytesSentPerSec:N0}
每秒接收字节数：{channel.BytesReceivedPerSec:N0}");
        }

        private void showNetStat(string content)
        {
            //如果内容未改变
            if (content != null && content == lastNetstatStr)
                return;
            txtNetstat.Text = content;
        }
    }
}
