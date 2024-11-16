using System;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.Connect
{
    /// <summary>
    /// 连接响应命令
    /// </summary>
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => ConnectCommandSerializerContext.Default.Response;

        /// <summary>
        /// 缓存大小。此参数对2.4以后版本不起作用。保留此参数为了兼容
        /// </summary>
        public int BufferSize { get; set; } = 128 * 1024;
        /// <summary>
        /// 认证问题
        /// </summary>
        public string Question { get; set; }
        public static Response GetDefine() => new Response() { Question = Guid.NewGuid().ToString("N") };
    }
}
