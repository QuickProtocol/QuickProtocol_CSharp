using Avalonia.Controls;
using Avalonia.Interactivity;
using QpTestClient.Utils;

namespace QpTestClient
{
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();
            Loaded += AboutBox_Loaded;
        }

        private void AboutBox_Loaded(object sender, RoutedEventArgs e)
        {
            Title = $"关于 {ProductInfoUtils.GetAssemblyTitle()}";
            LabelProductName.Text = ProductInfoUtils.GetAssemblyProduct();
            LabelVersion.Text = $"版本 {ProductInfoUtils.GetAssemblyVersion()}";
            LabelCopyright.Text = ProductInfoUtils.GetAssemblyCopyright();
            LabelCompanyName.Text = ProductInfoUtils.GetAssemblyCompany();
            TextBoxDescription.Text = ProductInfoUtils.GetAssemblyDescription();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
