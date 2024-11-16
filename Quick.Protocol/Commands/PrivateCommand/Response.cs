using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.PrivateCommand
{
    /// <summary>
    /// 私有命令响应
    /// </summary>
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => PrivateCommandCommandSerializerContext.Default.Response;
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        public static Response GetDefine() => new Response() { Content = "Content" };
    }
}
