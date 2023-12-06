using Quick.Protocol.Model;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.Connect
{
    /// <summary>
    /// 连接请求命令
    /// </summary>
    [DisplayName("连接")]
    public class Request : AbstractQpModel<Request>, IQpCommandRequest<Response>
    {
        protected override JsonTypeInfo<Request> TypeInfo => ConnectCommandSerializerContext.Default.Request;
        /// <summary>
        /// 指令集编号数组
        /// </summary>
        public string[] InstructionIds { get; set; }
    }
}
