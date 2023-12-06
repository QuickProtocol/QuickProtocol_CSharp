using Quick.Protocol.Model;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.GetQpInstructions
{
    /// <summary>
    /// 获取全部指令集信息请求
    /// </summary>
    [DisplayName("获取全部指令集信息")]
    public class Request : AbstractQpModel<Request>, IQpCommandRequest<Response>
    {
        protected override JsonTypeInfo<Request> TypeInfo => GetQpInstructionsCommandSerializerContext.Default.Request;
    }
}
