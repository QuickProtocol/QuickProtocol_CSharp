using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using QpTestClient.Utils;
using System;
using System.IO;
using System.Reflection;

namespace QpTestClient
{
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();
            Loaded += AboutBox_Loaded;
        }

        private void AboutBox_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream($"{nameof(QpTestClient)}.Images.logo-large.png");
                if (stream != null)
                    LogoImage.Source = new Bitmap(stream);
            }
            catch { }

            Title = $"关于 {ProductInfoUtils.GetAssemblyTitle()}";
            LabelProductName.Text = ProductInfoUtils.GetAssemblyProduct();
            LabelVersion.Text = $"版本 {ProductInfoUtils.GetAssemblyVersion()}";
            LabelCopyright.Text = ProductInfoUtils.GetAssemblyCopyright();
            LabelCompanyName.Text = ProductInfoUtils.GetAssemblyCompany();
            TextBoxDescription.Text = ProductInfoUtils.GetAssemblyDescription();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
