using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.HandShake
{
    [DisplayName("握手")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => HandShakeCommandSerializerContext.Default.Request;

        /// <summary>
        /// 传输超时(默认15秒)
        /// </summary>
        public int TransportTimeout { get; set; } = 15000;
        /// <summary>
        /// 启用加密(默认为false)
        /// </summary>
        public bool EnableEncrypt { get; set; } = false;
        /// <summary>
        /// 启用压缩(默认为false)
        /// </summary>
        public bool EnableCompress { get; set; } = false;
    }
}
