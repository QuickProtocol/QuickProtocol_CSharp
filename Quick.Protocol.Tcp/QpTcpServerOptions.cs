using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Quick.Protocol.Tcp
{
    public class QpTcpServerOptions : QpServerOptions
    {
        /// <summary>
        /// IP地址
        /// </summary>
        [JsonIgnore]
        public IPAddress Address { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        public override void Check()
        {
            base.Check();
            if (Address == null)
                throw new ArgumentNullException(nameof(Address));
            if (Port < 0 || Port > 65535)
                throw new ArgumentException("Port must between 0 and 65535", nameof(Port));
        }
    }
}
