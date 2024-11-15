using Quick.Protocol;
using System.Text;
using System.Windows.Forms;

namespace QpTestClient.Controls
{
    public partial class CommandInfoControl : UserControl
    {
        private QpCommandInfo item;
        public CommandInfoControl(QpCommandInfo item)
        {
            this.item = item;
            InitializeComponent();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"命令名称：{item.Name}");
            sb.AppendLine($"请求类名称：{item.RequestTypeName}");
            sb.AppendLine($"响应类名称：{item.ResponseTypeName}");
            if (!string.IsNullOrEmpty(item.Description))
            {
                sb.AppendLine("描述:");
                sb.AppendLine("---------------------");
                sb.AppendLine(item.Description);
            }
            txtBasic.Text = sb.ToString();
            txtRequestSchemaSample.Text = item.RequestTypeSchemaSample;
            txtResponseSchemaSample.Text = item.ResponseTypeSchemaSample;
        }
    }
}
