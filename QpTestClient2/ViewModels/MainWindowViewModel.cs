using System.Reflection;
using QpTestClient2.Controls;

namespace QpTestClient2.ViewModels
{
    public class MainWindowViewModel : PropertyNotifyModel
    {
        public MessageBoxViewModel MessageBox { get; set; } = new MessageBoxViewModel()
        {
            ButtonOkText = "确定",
        };

        public string Title { get; set; }

        public MainWindowViewModel()
        {
            Assembly assembly = GetType().Assembly;
            Title = $"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";
        }
    }
}
