using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Quick.Protocol;
using System;
using System.Linq;

namespace QpTestClient.Forms
{
    public partial class NoticeRecvForm : Window
    {
        private readonly ConnectionContext connectionContext;
        private QpClient? client;
        private string? noticeTypeName;
        private int maxLines;

        public NoticeRecvForm(ConnectionContext connectionContext, QpNoticeInfo? noticeInfo = null)
        {
            this.connectionContext = connectionContext;
            InitializeComponent();

            if (noticeInfo == null)
            {
                TxtFormTitle.Text = $"{Title} - {connectionContext.ConnectionInfo.Name}";
                TxtNoticeTypeName.Text = "*";
            }
            else
            {
                TxtFormTitle.Text = $"{Title} - {noticeInfo.Name} - {connectionContext.ConnectionInfo.Name}";
                TxtNoticeTypeName.Text = noticeInfo.NoticeTypeName;
            }
            Closing += NoticeRecvForm_Closing;
        }

        private void NoticeRecvForm_Closing(object? sender, WindowClosingEventArgs e)
        {
            BtnStopRecv_Click(sender, new RoutedEventArgs());
        }

        private void TxtFormTitle_TextChanged(object? sender, TextChangedEventArgs e)
        {
            Title = TxtFormTitle.Text?.Trim() ?? "通知接收";
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
            TxtNoticeTypeName.IsEnabled = false;
            NudMaxLines.IsEnabled = false;
            BtnStartRecv.IsEnabled = false;
            BtnStopRecv.IsEnabled = true;

            noticeTypeName = TxtNoticeTypeName.Text?.Trim();
            maxLines = Convert.ToInt32(NudMaxLines.Value ?? 100);
            PushLog("开始接收..");
            client.Disconnected += Client_Disconnected;
            client.RawNoticePackageReceived += Client_RawNoticePackageReceived;
        }

        private void Client_Disconnected(object? sender, EventArgs e)
        {
            PushLog("连接已断开!");
            Dispatcher.UIThread.Post(() => BtnStopRecv_Click(sender, new RoutedEventArgs()));
        }

        private void Client_RawNoticePackageReceived(object? sender, RawNoticePackageReceivedEventArgs e)
        {
            if (noticeTypeName != "*" && e.TypeName != noticeTypeName)
                return;
            if (noticeTypeName == "*")
                PushLog($"Type:{e.TypeName},Content:{e.Content}");
            else
                PushLog(e.Content);
        }

        private void BtnStopRecv_Click(object? sender, RoutedEventArgs e)
        {
            TxtFormTitle.IsEnabled = true;
            TxtNoticeTypeName.IsEnabled = true;
            NudMaxLines.IsEnabled = true;
            BtnStartRecv.IsEnabled = true;
            BtnStopRecv.IsEnabled = false;

            if (client != null)
            {
                client.Disconnected -= Client_Disconnected;
                client.RawNoticePackageReceived -= Client_RawNoticePackageReceived;
                client = null;
            }
            PushLog("已停止接收.");
        }
    }
}
