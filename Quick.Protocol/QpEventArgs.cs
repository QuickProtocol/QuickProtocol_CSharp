using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol
{
    public class QpEventArgs : EventArgs
    {
    }

    /// <summary>
    /// 原始收到通知数据包事件参数
    /// </summary>
    public class RawNoticePackageReceivedEventArgs : QpEventArgs
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 收到通知数据包事件参数
    /// </summary>
    public class NoticePackageReceivedEventArgs : QpEventArgs
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 内容模型
        /// </summary>
        public object ContentModel { get; set; }
    }

    /// <summary>
    /// 原始收到命令请求数据包事件参数
    /// </summary>
    public class RawCommandRequestPackageReceivedEventArgs : QpEventArgs
    {
        /// <summary>
        /// 命令编号
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled { get; set; } = false;
    }

    /// <summary>
    /// 收到命令请求数据包事件参数
    /// </summary>
    public class CommandRequestPackageReceivedEventArgs : QpEventArgs
    {
        /// <summary>
        /// 命令编号
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 内容模型
        /// </summary>
        public object ContentModel { get; set; }
    }


    /// <summary>
    /// 原始收到命令响应数据包事件参数
    /// </summary>
    public class RawCommandResponsePackageReceivedEventArgs : QpEventArgs
    {
        /// <summary>
        /// 命令编号
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// 响应码
        /// </summary>
        public byte Code { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 命令响应中的类型名称和内容
    /// </summary>
    public class CommandResponseTypeNameAndContent : QpEventArgs
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 收到命令响应数据包事件参数
    /// </summary>
    public class CommandResponsePackageReceivedEventArgs : CommandResponseTypeNameAndContent
    {
        /// <summary>
        /// 命令编号
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// 响应码
        /// </summary>
        public byte Code { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }        
    }
}
