using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.GetQpInstructions
{
    /// <summary>
    /// 获取全部指令集信息请求
    /// </summary>
    [DisplayName("获取全部指令集信息")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => GetQpInstructionsCommandSerializerContext.Default2.Request;
        public static Request GetDefine() => new Request();
    }
}
