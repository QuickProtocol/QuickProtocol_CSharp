using System.Text.Json;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Quick.Protocol
{
    public abstract class QpServerOptions : QpChannelOptions
    {
        /// <summary>
        /// 缓存大小(默认128KB)
        /// </summary>
        public int BufferSize = 128 * 1024;
        /// <summary>
        /// 认证超时时间，在指定的超时时间没有完成认证，则断开连接
        /// </summary>
        public int AuthenticateTimeout { get; set; } = 5000;
        /// <summary>
        /// 服务端程序
        /// </summary>
        public string ServerProgram { get; set; }

        /// <summary>
        /// 协议错误处理器
        /// </summary>
        [JsonIgnore]
        public Action<Stream, ArraySegment<byte>> ProtocolErrorHandler { get; set; }

        /// <summary>
        /// 创建客户端实例
        /// </summary>
        /// <returns></returns>
        public virtual QpServer CreateServer()
        {
            throw new NotImplementedException();
        }

        public virtual QpServerOptions Clone()
        {
            var type = this.GetType();
            var ret = (QpServerOptions)JsonSerializer.Deserialize(
                JsonSerializer.Serialize(this, type, JsonSerializerContext), type, JsonSerializerContext);
            ret.InstructionSet = InstructionSet;
            ret.CommandExecuterManagerList = CommandExecuterManagerList;
            ret.NoticeHandlerManagerList = NoticeHandlerManagerList;
            ret.ProtocolErrorHandler = ProtocolErrorHandler;
            return ret;
        }
    }
}
