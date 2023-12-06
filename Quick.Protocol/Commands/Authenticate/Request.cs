using Quick.Protocol.Model;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.Authenticate
{
    [DisplayName("认证")]
    public class Request : AbstractQpModel<Request>, IQpCommandRequest<Response>
    {
        protected override JsonTypeInfo<Request> TypeInfo => AuthenticateCommandSerializerContext.Default.Request;

        /// 认证回答
        /// </summary>
        public string Answer { get; set; }
    }
}
