using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol
{
    /// <summary>
    /// 数据包类型
    /// </summary>
    public enum QpPackageType : byte
    {
        /// <summary>
        /// 心跳数据包
        /// </summary>
        Heartbeat = 0,
        /// <summary>
        /// 通知数据包
        /// </summary>
        Notice = 1,
        /// <summary>
        /// 指令请求数据包
        /// </summary>
        CommandRequest = 2,
        /// <summary>
        /// 指令响应数据包
        /// </summary>
        CommandResponse = 3,
        /// <summary>
        /// 拆分数据包
        /// </summary>
        Split = 255
    }
}
