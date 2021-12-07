using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.Commands.GetQpInstructions
{
    /// <summary>
    /// 获取全部指令集信息响应
    /// </summary>
    public class Response
    {
        /// <summary>
        /// 指令集数据
        /// </summary>
        public QpInstruction[] Data { get; set; }
    }
}
