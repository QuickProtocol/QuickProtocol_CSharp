using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.Commands.Connect
{
    /// <summary>
    /// 连接响应命令
    /// </summary>
    public class Response
    {
        /// <summary>
        /// 缓存大小
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// 认证问题
        /// </summary>
        public string Question { get; set; }
    }
}
