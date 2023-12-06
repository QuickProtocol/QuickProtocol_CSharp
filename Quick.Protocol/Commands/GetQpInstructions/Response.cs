using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.GetQpInstructions
{
    /// <summary>
    /// 获取全部指令集信息响应
    /// </summary>
    public class Response: AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => GetQpInstructionsCommandSerializerContext.Default.Response;
        /// <summary>
        /// 指令集数据
        /// </summary>
        public QpInstruction[] Data { get; set; }
    }
}
