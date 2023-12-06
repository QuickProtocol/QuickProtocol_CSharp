using Quick.Protocol.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.PrivateCommand
{
    /// <summary>
    /// 私有命令响应
    /// </summary>
    public class Response : AbstractQpModel<Response>
    {
        protected override JsonTypeInfo<Response> TypeInfo => PrivateCommandCommandSerializerContext.Default.Response;
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
