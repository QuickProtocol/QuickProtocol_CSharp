using Avalonia.Controls;
using Quick.Protocol;
using System.Text;

namespace QpTestClient.Controls
{
    public partial class CommandInfoControl : UserControl
    {
        public CommandInfoControl(QpCommandInfo item)
        {
            InitializeComponent();

            var sb = new StringBuilder();
            sb.AppendLine($"命令名称：{item.Name}");
            sb.AppendLine($"请求类名称：{item.RequestTypeName}");
            sb.AppendLine($"响应类名称：{item.ResponseTypeName}");
            if (!string.IsNullOrEmpty(item.Description))
            {
                sb.AppendLine("描述:");
                sb.AppendLine("---------------------");
                sb.AppendLine(item.Description);
            }
            TxtBasic.Text = sb.ToString();
            TxtRequestSchemaSample.Text = item.RequestTypeSchemaSample;
            TxtResponseSchemaSample.Text = item.ResponseTypeSchemaSample;
        }
    }
}
