﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.WebSocket.Client
{
    [DisplayName("WebSocket")]
    public class QpWebSocketClient : QpClient
    {
        private QpWebSocketClientOptions options;
        private System.Net.WebSockets.ClientWebSocket client;

        public QpWebSocketClient(QpWebSocketClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            client = new System.Net.WebSockets.ClientWebSocket();
            var url = options.Url;
            if (url.StartsWith("qp."))
                url = url.Substring(3);
            var cts = new CancellationTokenSource();
            try
            {
                await client.ConnectAsync(new Uri(url), cts.Token).ConfigureAwait(false);
            }
            catch
            {
                cts.Cancel();
                client.Dispose();
                throw;
            }            
            return new WebSocketClientStream(client);
        }
    }
}
