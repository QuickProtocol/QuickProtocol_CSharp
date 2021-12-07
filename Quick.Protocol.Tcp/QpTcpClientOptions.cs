using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.Tcp
{
    public class QpTcpClientOptions : QpClientOptions
    {
        /// <summary>
        /// 主机
        /// </summary>
        [DisplayName("主机")]
        [Category("常用")]
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口
        /// </summary>
        [DisplayName("端口")]
        [Category("常用")]
        public int Port { get; set; } = 3011;
        /// <summary>
        /// 本地主机
        /// </summary>
        [DisplayName("本地主机")]
        [Category("高级")]
        public string LocalHost { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        [DisplayName("本地端口")]
        [Category("高级")]
        public int LocalPort { get; set; } = 0;

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(Host))
                throw new ArgumentNullException(nameof(Host));
            if (Port < 0 || Port > 65535)
                throw new ArgumentException("Port must between 0 and 65535", nameof(Port));
        }

        public override string GetConnectionInfo() => $"{Host}:{Port}";
    }
}
