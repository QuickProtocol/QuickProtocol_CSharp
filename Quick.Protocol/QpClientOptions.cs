using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol
{
    public abstract class QpClientOptions : QpChannelOptions
    {
        /// <summary>
        /// 连接超时(默认为5秒)
        /// </summary>
        [DisplayName("连接超时")]
        [Category("高级")]
        public int ConnectionTimeout { get; set; } = 5 * 1000;
        /// <summary>
        /// 传输超时(默认15秒)
        /// </summary>
        [DisplayName("传输超时")]
        [Category("高级")]
        public int TransportTimeout
        {
            get { return InternalTransportTimeout; }
            set { InternalTransportTimeout = value; }
        }

        /// <summary>
        /// 启用加密(默认为false)
        /// </summary>
        [DisplayName("启用加密")]
        [Category("高级")]
        public bool EnableEncrypt { get; set; } = false;
        /// <summary>
        /// 启用压缩(默认为false)
        /// </summary>
        [DisplayName("启用压缩")]
        [Category("高级")]
        public bool EnableCompress { get; set; } = false;

        /// <summary>
        /// 当认证通过时
        /// </summary>
        public void OnAuthPassed()
        {
            InternalCompress = EnableCompress;
            InternalEncrypt = EnableEncrypt;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InternalCompress = false;
            InternalEncrypt = false;
        }

        /// <summary>
        /// 获取连接信息
        /// </summary>
        /// <returns></returns>
        public abstract string GetConnectionInfo();

        public override string ToString() => GetConnectionInfo();
    }
}
