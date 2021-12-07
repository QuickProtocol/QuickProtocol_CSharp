using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QpTestClient.Forms
{
    public partial class HeartbeatRecvForm : Form
    {
        private ConnectionContext connectionContext;
        private QpClient client;
        private int maxLines;

        public HeartbeatRecvForm(ConnectionContext connectionContext)
        {
            this.connectionContext = connectionContext;
            InitializeComponent();
            txtFormTitle.Text = $"{Text} - {connectionContext.ConnectionInfo.Name}";
        }

        private void HeartbeatRecvForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnStopRecv_Click(sender, e);
        }

        private void txtFormTitle_TextChanged(object sender, EventArgs e)
        {
            Text = txtFormTitle.Text;
        }

        private void pushLog(string line)
        {
            Invoke(new Action(() =>
            {
                txtLog.AppendText($"{DateTime.Now.ToLongTimeString()}: {line}{Environment.NewLine}");
                var lines = txtLog.Lines;
                if (lines.Length > maxLines)
                    txtLog.Lines = lines.Skip(lines.Length - maxLines).ToArray();
                txtLog.Select(txtLog.TextLength, 0);
                txtLog.ScrollToCaret();
            }));
        }

        private void btnStartRecv_Click(object sender, EventArgs e)
        {
            client = connectionContext.QpClient;
            if (client == null)
            {
                pushLog($"当前未连接，无法接收！");
                return;
            }

            txtFormTitle.Enabled = false;
            nudMaxLines.Enabled = false;
            btnStartRecv.Enabled = false;
            btnStopRecv.Enabled = true;

            maxLines = Convert.ToInt32(nudMaxLines.Value);
            pushLog("开始接收..");
            client.Disconnected += Client_Disconnected;
            client.HeartbeatPackageReceived += Client_HeartbeatPackageReceived;
            
        }

        private void Client_HeartbeatPackageReceived(object sender, EventArgs e)
        {
            pushLog("收到心跳数据包");
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            pushLog("连接已断开!");
            Invoke(new Action(() => btnStopRecv_Click(sender, e)));
        }

        private void btnStopRecv_Click(object sender, EventArgs e)
        {
            txtFormTitle.Enabled = true;
            nudMaxLines.Enabled = true;
            btnStartRecv.Enabled = true;
            btnStopRecv.Enabled = false;

            if (client != null)
            {
                client.Disconnected -= Client_Disconnected;
                client.HeartbeatPackageReceived -= Client_HeartbeatPackageReceived;
                client = null;
            }
            pushLog("已停止接收.");
        }
    }
}
