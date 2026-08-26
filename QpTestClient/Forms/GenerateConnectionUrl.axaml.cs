using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace QpTestClient.Forms
{
    public partial class GenerateConnectionUrl : Window
    {
        private readonly ConnectionContext connectionContext;

        public GenerateConnectionUrl(ConnectionContext connectionContext)
        {
            this.connectionContext = connectionContext;
            InitializeComponent();
            Loaded += GenerateConnectionUrl_Loaded;
        }

        private void GenerateConnectionUrl_Loaded(object? sender, RoutedEventArgs e)
        {
            Title = $"生成[{connectionContext.ConnectionInfo.Name}]连接URL";
            Generate();
        }

        private void Checkbox_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void Generate()
        {
            var includePassword = CbIncludePassword.IsChecked ?? false;
            var includeOtherProperty = CbIncludeOtherProperty.IsChecked ?? false;

            TxtOutput.Text = connectionContext.ConnectionInfo.QpClientOptions.ToUri(
                includePassword,
                includeOtherProperty
            ).ToString();
        }

        private async void BtnCopy_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(TxtOutput.Text);
                await MessageBox.Show(this, "已复制到剪贴板！", "提示");
            }
        }
    }
}
