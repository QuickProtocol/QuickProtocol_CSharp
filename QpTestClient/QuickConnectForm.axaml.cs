using Avalonia.Controls;
using Avalonia.Interactivity;
using Quick.Utils;
using System;

namespace QpTestClient
{
    public partial class QuickConnectForm : Window
    {
        public TestConnectionInfo? ConnectionInfo { get; private set; }

        public QuickConnectForm()
        {
            InitializeComponent();
            Loaded += QuickConnectForm_Loaded;
        }

        private void QuickConnectForm_Loaded(object? sender, RoutedEventArgs e)
        {
            TxtName.Text = "快速添加连接_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        private void TxtUrl_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var url = TxtUrl.Text?.Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                PnlPassword.IsVisible = true;
                return;
            }
            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
            PnlPassword.IsVisible = string.IsNullOrEmpty(queryString.Get("Password"));
        }

        private void BtnOk_Click(object? sender, RoutedEventArgs e)
        {
            var name = TxtName.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                // TODO: Show validation message
                TxtName.Focus();
                return;
            }

            var url = TxtUrl.Text?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                // TODO: Show validation message
                TxtUrl.Focus();
                return;
            }

            var password = TxtPassword.Text?.Trim();
            if (PnlPassword.IsVisible)
            {
                if (string.IsNullOrEmpty(password))
                {
                    // TODO: Show validation message
                    TxtPassword.Focus();
                    return;
                }
            }

            var uri = new Uri(url);
            Quick.Protocol.QpClientOptions? options = null;
            try
            {
                options = Quick.Protocol.QpClientOptions.Parse(uri);
            }
            catch (Exception ex)
            {
                // TODO: Show error message
                return;
            }

            if (PnlPassword.IsVisible)
                options.Password = password;

            ConnectionInfo = new TestConnectionInfo()
            {
                Name = name,
                QpClientTypeName = options.CreateClient().GetType().FullName!,
                QpClientOptions = options
            };
            Close(ConnectionInfo);
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
