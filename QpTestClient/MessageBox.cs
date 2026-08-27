using Avalonia.Controls;
using System.Threading.Tasks;

namespace QpTestClient
{
    public enum MessageBoxButtons
    {
        OK,
        YesNo
    }

    public enum MessageBoxResult
    {
        OK,
        Yes,
        No,
        Cancel
    }

    public static class MessageBox
    {
        public static async Task<MessageBoxResult> Show(Window owner, string message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var result = MessageBoxResult.Cancel;

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(20),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var okButton = new Button
            {
                Content = "确定(_O)",
                Width = 100,
                Height = 30,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var yesButton = new Button
            {
                Content = "是(_Y)",
                Width = 100,
                Height = 30,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var noButton = new Button
            {
                Content = "否(_N)",
                Width = 100,
                Height = 30,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (_, _) =>
            {
                result = MessageBoxResult.OK;
                dialog.Close();
            };

            yesButton.Click += (_, _) =>
            {
                result = MessageBoxResult.Yes;
                dialog.Close();
            };

            noButton.Click += (_, _) =>
            {
                result = MessageBoxResult.No;
                dialog.Close();
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10,
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            };

            if (buttons == MessageBoxButtons.OK)
            {
                buttonPanel.Children.Add(okButton);
            }
            else
            {
                buttonPanel.Children.Add(yesButton);
                buttonPanel.Children.Add(noButton);
            }

            var mainPanel = new DockPanel();
            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            mainPanel.Children.Add(buttonPanel);
            mainPanel.Children.Add(messageText);

            dialog.Content = mainPanel;

            await dialog.ShowDialog(owner);
            return result;
        }
    }
}
