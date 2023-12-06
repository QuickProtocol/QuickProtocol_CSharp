using Quick.Protocol.Model;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.HandShake
{
    public class Response : AbstractQpModel<Response>
    {
        protected override JsonTypeInfo<Response> TypeInfo => HandShakeCommandSerializerContext.Default.Response;
    }
}
