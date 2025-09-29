using Microsoft.AspNetCore.Http;
using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.WebSocket.Server.AspNetCore
{
    public class QpWebSocketServer : QpServer
    {
        private Queue<WebSocketContext> webSocketContextQueue = new Queue<WebSocketContext>();
        private bool isStarted = false;

        private class WebSocketContext
        {
            public string ConnectionInfo { get; set; }
            public System.Net.WebSockets.WebSocket WebSocket { get; set; }
            public CancellationTokenSource Cts { get; set; }

            public WebSocketContext(string connectionInfo, System.Net.WebSockets.WebSocket webSocket, CancellationTokenSource cts)
            {
                ConnectionInfo = connectionInfo;
                WebSocket = webSocket;
                Cts = cts;
            }
        }

        public QpWebSocketServer(QpWebSocketServerOptions options) : base(options) { }

        public override void Start()
        {
            isStarted = true;
            lock (webSocketContextQueue)
            {
                foreach (var webSocket in webSocketContextQueue)
                    webSocket.Cts.Cancel();
                webSocketContextQueue.Clear();
            }
            base.Start();
        }

        public async Task OnNewConnection(System.Net.WebSockets.WebSocket webSocket, ConnectionInfo connectionInfo)
        {
            //如果还没有开始接收，则直接关闭
            if (!isStarted)
            {
                await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return;
            }

            var connectionInfoStr = $"WebSocket:{connectionInfo.RemoteIpAddress}:{connectionInfo.RemotePort}";
            var cts = new CancellationTokenSource();
            lock (webSocketContextQueue)
                webSocketContextQueue.Enqueue(
                    new WebSocketContext(
                        connectionInfoStr,
                        webSocket,
                        cts));
            await Task.Delay(-1, cts.Token).ContinueWith(t =>
             {
                 if (LogUtils.LogConnection)
                     LogUtils.Log("[Connection]{0} disconnected.", connectionInfoStr);
             });
        }

        public override void Stop()
        {
            isStarted = false;
            lock (webSocketContextQueue)
            {
                foreach (var webSocket in webSocketContextQueue)
                    webSocket.Cts.Cancel();
                webSocketContextQueue.Clear();
            }
            base.Stop();
        }

        protected override async Task InnerAcceptAsync(CancellationToken token)
        {
            WebSocketContext[] webSocketContexts = null;
            lock (webSocketContextQueue)
            {
                webSocketContexts = webSocketContextQueue.ToArray();
                webSocketContextQueue.Clear();
            }
            //如果当前没有WebSocket连接，则等待0.1秒后再返回
            if (webSocketContexts == null || webSocketContexts.Length == 0)
            {
                await Task.Delay(100).ConfigureAwait(false);
                return;
            }
            foreach (var context in webSocketContexts)
            {
                try
                {
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]{0} connected.", context.ConnectionInfo);
                    OnNewChannelConnected(new WebSocketServerStream(context.WebSocket, context.Cts), context.ConnectionInfo, token);
                }
                catch (Exception ex)
                {
                    context.Cts.Cancel();
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]Init&Start Channel error,reason:{0}", ex.ToString());
                    try
                    {
                        await context.WebSocket
                            .CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                    catch { }
                }
            }
        }
    }
}
