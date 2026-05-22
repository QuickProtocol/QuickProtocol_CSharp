using Avalonia.Controls;
using Avalonia.Threading;
using Quick.Protocol;

namespace QpTestClient;

public partial class HeartbeatRecvForm : Window
{
    private ConnectionContext connectionContext;
    private QpClient client;
    private int maxLines;

    public HeartbeatRecvForm(ConnectionContext connectionContext)
    {
        this.connectionContext = connectionContext;
        InitializeComponent();
        txtFormTitle.Text = $"{Title} - {connectionContext.ConnectionInfo.Name}";
    }

    private void txtFormTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        Title = txtFormTitle.Text;
    }

    private List<string> logList = new List<string>();

    private void pushLog(string line)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            logList.Add($"{DateTime.Now.ToLongTimeString()}: {line}");
            while (logList.Count > maxLines)
            {
                logList.RemoveAt(0);
            }
            var logText = string.Join(Environment.NewLine, logList);
            txtLog.Text = logText;
            txtLog.ScrollToLine(logList.Count - 1);
        });
    }


    private void btnStartRecv_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        client = connectionContext.QpClient;
        if (client == null)
        {
            pushLog($"当前未连接，无法接收！");
            return;
        }

        txtFormTitle.IsEnabled = false;
        nudMaxLines.IsEnabled = false;
        btnStartRecv.IsEnabled = false;
        btnStopRecv.IsEnabled = true;

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
        Dispatcher.UIThread.Invoke(() => btnStopRecv_Click(sender, null));
    }

    private void btnStopRecv_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        txtFormTitle.IsEnabled = true;
        nudMaxLines.IsEnabled = true;
        btnStartRecv.IsEnabled = true;
        btnStopRecv.IsEnabled = false;

        if (client != null)
        {
            client.Disconnected -= Client_Disconnected;
            client.HeartbeatPackageReceived -= Client_HeartbeatPackageReceived;
            client = null;
        }
        pushLog("已停止接收.");
    }

    private void Window_Closing(object sender, WindowClosingEventArgs e)
    {
        btnStopRecv_Click(sender, null);
    }
}