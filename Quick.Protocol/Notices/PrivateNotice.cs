using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.Notices
{
    [DisplayName("私有通知")]
    [Description("用于传递私有协议通知。")]
    public class PrivateNotice
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
