using System;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.Authenticate
{
    [DisplayName("认证")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => AuthenticateCommandSerializerContext.Default.Request;

        /// 认证回答
        /// </summary>
        public string Answer { get; set; }

        public static Request GetDefine() => new Request() { Answer = Guid.NewGuid().ToString("N") };
    }
}
