using Quick.Protocol.Model;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.GetQpInstructions
{
    /// <summary>
    /// 获取全部指令集信息响应
    /// </summary>
    public class Response: AbstractQpModel<Response>
    {
        protected override JsonTypeInfo<Response> TypeInfo => GetQpInstructionsCommandSerializerContext.Default.Response;
        /// <summary>
        /// 指令集数据
        /// </summary>
        public QpInstruction[] Data { get; set; }
    }
}
