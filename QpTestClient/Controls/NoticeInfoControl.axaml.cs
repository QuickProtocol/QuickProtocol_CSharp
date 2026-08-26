using Avalonia.Controls;
using Quick.Protocol;
using System.Text;

namespace QpTestClient.Controls
{
    public partial class NoticeInfoControl : UserControl
    {
        public NoticeInfoControl(QpNoticeInfo item)
        {
            InitializeComponent();

            var sb = new StringBuilder();
            sb.AppendLine($"通知名称：{item.Name}");
            sb.AppendLine($"类名称：{item.NoticeTypeName}");
            if (!string.IsNullOrEmpty(item.Description))
            {
                sb.AppendLine("描述:");
                sb.AppendLine("---------------------");
                sb.AppendLine(item.Description);
            }
            TxtBasic.Text = sb.ToString();
            TxtSchemaSample.Text = item.NoticeTypeSchemaSample;
        }
    }
}
