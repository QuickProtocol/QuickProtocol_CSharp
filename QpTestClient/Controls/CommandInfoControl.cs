using Quick.Protocol;
using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            sb.AppendLine($"名称：{item.Name}");
            sb.AppendLine($"请求类名称：{item.RequestTypeName}");
            sb.AppendLine($"响应类名称：{item.ResponseTypeName}");
            if (!string.IsNullOrEmpty(item.Description))
            {
                sb.AppendLine("描述:");
                sb.AppendLine("---------------------");
                sb.AppendLine(item.Description);
            }
            txtBasic.Text = sb.ToString();
            txtRequestSchema.Text = item.RequestTypeSchema;
            txtRequestSchemaSample.Text = item.RequestTypeSchemaSample;
            txtResponseSchema.Text = item.ResponseTypeSchema;
            txtResponseSchemaSample.Text = item.ResponseTypeSchemaSample;
        }
    }
}
