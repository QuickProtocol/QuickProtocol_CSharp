using Quick.Protocol.Model;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.PrivateCommand
{
    /// <summary>
    /// 私有命令请求
    /// </summary>
    [DisplayName("私有命令")]
    public class Request : AbstractQpModel<Request>, IQpCommandRequest<Response>
    {
        protected override JsonTypeInfo<Request> TypeInfo => PrivateCommandCommandSerializerContext.Default.Request;
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
