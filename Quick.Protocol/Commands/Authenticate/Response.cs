using System;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.Authenticate
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => AuthenticateCommandSerializerContext.Default.Response;
        public static Response GetDefine() => new Response();
    }
}
