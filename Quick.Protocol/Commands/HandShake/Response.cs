using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.HandShake
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => HandShakeCommandSerializerContext.Default2.Response;
        public static Response GetDefine() => new Response();
    }
}
