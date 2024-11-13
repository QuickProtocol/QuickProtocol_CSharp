using QpTestClient.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            var currentAssembly = this.GetType().Assembly;
            //窗体图标
            using (var stream = currentAssembly.GetManifestResourceStream($"{nameof(QpTestClient)}.Images.logo-large.png"))
                logoPictureBox.Image = Image.FromStream(stream);
            this.Text = String.Format("关于 {0}", ProductInfoUtils.GetAssemblyTitle());
            this.labelProductName.Text = ProductInfoUtils.GetAssemblyProduct();
            this.labelVersion.Text = $"版本 {ProductInfoUtils.GetAssemblyVersion()}";
            this.labelCopyright.Text = ProductInfoUtils.GetAssemblyCopyright();
            this.labelCompanyName.Text = ProductInfoUtils.GetAssemblyCompany();
            this.textBoxDescription.Text = ProductInfoUtils.GetAssemblyDescription();
        }
    }
}
