using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Commands.HandShake
{
    [DisplayName("握手")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => HandShakeCommandSerializerContext.Default2.Request;

        /// <summary>
        /// 传输超时(默认15秒)
        /// </summary>
        public int TransportTimeout { get; set; } = 15000;
        /// <summary>
        /// 启用加密(默认为false)
        /// </summary>
        public bool EnableEncrypt { get; set; } = false;
        /// <summary>
        /// 加密方式
        /// </summary>
        public string EncryptMethod { get; set; } = "DES";
        /// <summary>
        /// 加密算法模式
        /// </summary>
        public string EncryptMode { get; set; } = "ECB";
        /// <summary>
        /// 加密填充模式
        /// </summary>
        public string EncryptPadding { get; set; } = "PKCS7";

        /// <summary>
        /// 启用压缩(默认为false)
        /// </summary>
        public bool EnableCompress { get; set; } = false;

        public static Request GetDefine() => new Request();
    }
}
