using Quick.Protocol.Model;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.Authenticate
{
    public class Response : AbstractQpModel<Response>
    {
        protected override JsonTypeInfo<Response> TypeInfo => AuthenticateCommandSerializerContext.Default.Response;

    }
}
