using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Quick.Protocol.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.WebSocket.Server.AspNetCore
{
    public class QpWebSocketServer : QpServer
    {
        private Queue<WebSocketContext> webSocketContextQueue = new Queue<WebSocketContext>();
        private AutoResetEvent waitForConnectionAutoResetEvent;

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
            waitForConnectionAutoResetEvent = new AutoResetEvent(false);
            lock (webSocketContextQueue)
                webSocketContextQueue.Clear();
            base.Start();
        }

        public Task OnNewConnection(System.Net.WebSockets.WebSocket webSocket, ConnectionInfo connectionInfo)
        {
            var connectionInfoStr = $"WebSocket:{connectionInfo.RemoteIpAddress}:{connectionInfo.RemotePort}";
            var cts = new CancellationTokenSource();
            lock (webSocketContextQueue)
                webSocketContextQueue.Enqueue(
                    new WebSocketContext(
                        connectionInfoStr,
                        webSocket,
                        cts));
            waitForConnectionAutoResetEvent.Set();
            return Task.Delay(-1, cts.Token).ContinueWith(t =>
             {
                 if (LogUtils.LogConnection)
                     Console.WriteLine("[Connection]{0} disconnected.", connectionInfoStr);
             });
        }

        public override void Stop()
        {
            lock (webSocketContextQueue)
                webSocketContextQueue.Clear();
            waitForConnectionAutoResetEvent.Dispose();
            base.Stop();
        }

        protected override Task InnerAcceptAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                waitForConnectionAutoResetEvent.WaitOne();
                WebSocketContext[] webSocketContexts = null;
                lock (webSocketContextQueue)
                {
                    webSocketContexts = webSocketContextQueue.ToArray();
                    webSocketContextQueue.Clear();
                }
                foreach (var context in webSocketContexts)
                {
                    try
                    {
                        if (LogUtils.LogConnection)
                            Console.WriteLine("[Connection]{0} connected.", context.ConnectionInfo);
                        OnNewChannelConnected(new WebSocketServerStream(context.WebSocket,context.Cts), context.ConnectionInfo, token);
                    }
                    catch (Exception ex)
                    {
                        if (LogUtils.LogConnection)
                            Console.WriteLine("[Connection]Init&Start Channel error,reason:{0}", ex.ToString());
                        try { context.WebSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None); }
                        catch { }
                    }
                }
            });
        }
    }
}
