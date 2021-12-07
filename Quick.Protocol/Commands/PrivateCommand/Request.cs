using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.Commands.PrivateCommand
{
    /// <summary>
    /// 私有命令请求
    /// </summary>
    [DisplayName("私有命令")]
    public class Request : IQpCommandRequest<Response>
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
