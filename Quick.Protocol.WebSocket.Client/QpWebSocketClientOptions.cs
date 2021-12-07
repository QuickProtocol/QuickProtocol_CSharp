using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.WebSocket.Client
{
    public class QpWebSocketClientOptions : QpClientOptions
    {
        /// <summary>
        /// WebSocket的URL地址
        /// </summary>
        [DisplayName("URL")]
        [Category("常用")]
        public string Url { get; set; } = "ws://127.0.0.1:3011/qp_test";

        public override void Check()
        {
            base.Check();
            if (Url == null)
                throw new ArgumentNullException(nameof(Url));
            if (!Url.StartsWith("ws://"))
                throw new ArgumentException("Url must start with ws://", nameof(Url));
        }

        public override string GetConnectionInfo() => Url;
    }
}
