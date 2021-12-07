using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.Protocol
{
    public class QpServerOptions : QpChannelOptions
    {
        /// <summary>
        /// 缓存大小(默认128KB)
        /// </summary>
        public int BufferSize = 128 * 1024;

        /// <summary>
        /// 服务端程序
        /// </summary>
        public string ServerProgram { get; set; }

        /// <summary>
        /// 协议错误处理器
        /// </summary>
        [JsonIgnore]
        public Action<Stream,ArraySegment<byte>> ProtocolErrorHandler { get; set; }

        public virtual QpServerOptions Clone()
        {
            var ret = JsonConvert.DeserializeObject<QpServerOptions>(JsonConvert.SerializeObject(this));
            ret.InstructionSet = InstructionSet;
            ret.CommandExecuterManagerList = CommandExecuterManagerList;
            ret.ProtocolErrorHandler = ProtocolErrorHandler;
            return ret;
        }
    }
}
