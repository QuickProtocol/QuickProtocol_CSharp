using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Quick.Protocol;
using System;
using System.Linq;

namespace QpTestClient.Forms
{
    public partial class HeartbeatRecvForm : Window
    {
        private readonly ConnectionContext connectionContext;
        private QpClient? client;
        private int maxLines;

        public HeartbeatRecvForm(ConnectionContext connectionContext)
        {
            this.connectionContext = connectionContext;
            InitializeComponent();
            TxtFormTitle.Text = $"{Title} - {connectionContext.ConnectionInfo.Name}";
            Closing += HeartbeatRecvForm_Closing;
        }

        private void HeartbeatRecvForm_Closing(object? sender, WindowClosingEventArgs e)
        {
            BtnStopRecv_Click(sender, new RoutedEventArgs());
        }

        private void TxtFormTitle_TextChanged(object? sender, TextChangedEventArgs e)
        {
            Title = TxtFormTitle.Text?.Trim() ?? "心跳接收";
        }

        private void PushLog(string line)
        {
            Dispatcher.UIThread.Post(() =>
            {
                TxtLog.Text += $"{DateTime.Now.ToLongTimeString()}: {line}{Environment.NewLine}";
                var lines = TxtLog.Text.Split(Environment.NewLine);
                if (lines.Length > maxLines)
                    TxtLog.Text = string.Join(Environment.NewLine, lines.Skip(lines.Length - maxLines));
            });
        }

        private void BtnStartRecv_Click(object? sender, RoutedEventArgs e)
        {
            client = connectionContext.QpClient;
            if (client == null)
            {
                PushLog("当前未连接，无法接收！");
                return;
            }

            TxtFormTitle.IsEnabled = false;
            NudMaxLines.IsEnabled = false;
            BtnStartRecv.IsEnabled = false;
            BtnStopRecv.IsEnabled = true;

            maxLines = Convert.ToInt32(NudMaxLines.Value ?? 100);
            PushLog("开始接收..");
            client.Disconnected += Client_Disconnected;
            client.HeartbeatPackageReceived += Client_HeartbeatPackageReceived;
        }

        private void Client_HeartbeatPackageReceived(object? sender, EventArgs e)
        {
            PushLog("收到心跳数据包");
        }

        private void Client_Disconnected(object? sender, EventArgs e)
        {
            PushLog("连接已断开!");
            Dispatcher.UIThread.Post(() => BtnStopRecv_Click(sender, new RoutedEventArgs()));
        }

        private void BtnStopRecv_Click(object? sender, RoutedEventArgs e)
        {
            TxtFormTitle.IsEnabled = true;
            NudMaxLines.IsEnabled = true;
            BtnStartRecv.IsEnabled = true;
            BtnStopRecv.IsEnabled = false;

            if (client != null)
            {
                client.Disconnected -= Client_Disconnected;
                client.HeartbeatPackageReceived -= Client_HeartbeatPackageReceived;
                client = null;
            }
            PushLog("已停止接收.");
        }
    }
}
