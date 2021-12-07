using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QpTestClient.Controls
{
    public partial class NoticeInfoControl : UserControl
    {
        public NoticeInfoControl(QpNoticeInfo item)
        {
            InitializeComponent();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"名称：{item.Name}");
            sb.AppendLine($"类名称：{item.NoticeTypeName}");            
            if (!string.IsNullOrEmpty(item.Description))
            {
                sb.AppendLine("描述:");
                sb.AppendLine("---------------------");
                sb.AppendLine(item.Description);
            }
            txtBasic.Text = sb.ToString();
            txtSchema.Text = item.NoticeTypeSchema;
            txtSchemaSample.Text = item.NoticeTypeSchemaSample;
        }
    }
}
