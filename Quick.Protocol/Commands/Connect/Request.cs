using System.ComponentModel;

namespace Quick.Protocol.Commands.Connect
{
    /// <summary>
    /// 连接请求命令
    /// </summary>
    [DisplayName("连接")]
    public class Request
    {
        /// <summary>
        /// 指令集编号数组
        /// </summary>
        public string[] InstructionIds { get; set; }
    }
}
