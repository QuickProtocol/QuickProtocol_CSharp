using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Quick.Protocol.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Http.Server.AspNetCore
{
    public class QpHttpServer : QpServer
    {
        public const string QP_CHANNEL_ID = nameof(QP_CHANNEL_ID);

        private QpHttpServerOptions options;

        private Queue<QpHttpContext> httpContextQueue = new Queue<QpHttpContext>();
        private Dictionary<string, QpHttpContext> httpContextDict = new();

        private bool isStarted = false;

        private class QpHttpContext
        {
            public string ChannelId { get; set; }
            public string ConnectionInfo { get; set; }
            public Stream Stream { get; set; }
            private Pipe readPipe;
            private Pipe writePipe;
            private QpHttpServerOptions options;
            private byte[] buffer;

            public QpHttpContext(QpHttpServerOptions options, string channelId, string connectionInfo, CancellationTokenSource cts)
            {
                this.options = options;
                ChannelId = channelId;
                ConnectionInfo = connectionInfo;
                readPipe = new();
                writePipe = new();
                Stream = new PipesStream(channelId, readPipe, writePipe);
                buffer = new byte[options.MaxHttpResponseSize];
            }

            public async Task OnDataRecvAsync(Stream body)
            {
                using (var writerStream = readPipe.Writer.AsStream(true))
                    await body.CopyToAsync(writerStream);
            }

            public async Task OnGetData(HttpResponse rep)
            {
                try
                {
                    var readCts = new CancellationTokenSource();
                    var readCancallationToken = readCts.Token;
                    _ = Task.Delay(options.LongPollingTimeout, readCancallationToken).ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                            return;
                        readCts.Cancel();
                    });
                    var readResult = await writePipe.Reader.ReadAsync(readCancallationToken);
                    readCts.Cancel();

                    rep.ContentType = "application/octet-stream";
                    rep.ContentLength = readResult.Buffer.Length;
                    var currentSeq = readResult.Buffer;
                    while (currentSeq.Length > 0)
                    {
                        var length = Math.Min(buffer.Length, (int)currentSeq.Length);
                        currentSeq.CopyTo(buffer);
                        await rep.Body.WriteAsync(buffer, 0, length);
                        currentSeq = currentSeq.Slice(length);
                    }
                    writePipe.Reader.AdvanceTo(readResult.Buffer.GetPosition(readResult.Buffer.Length));
                }
                catch (OperationCanceledException)
                {
                    rep.StatusCode = 200;
                    rep.ContentLength = 0;
                    await rep.CompleteAsync();
                }
            }
        }

        public QpHttpServer(QpHttpServerOptions options) : base(options)
        {
            this.options = options;
        }

        public override void Start()
        {
            isStarted = true;
            lock (httpContextQueue)
                httpContextQueue.Clear();
            ChannelAuchenticateTimeout += OnChannelAuchenticateTimeoutOrDisconnected;
            ChannelDisconnected += OnChannelAuchenticateTimeoutOrDisconnected;
            base.Start();
        }

        private void OnChannelAuchenticateTimeoutOrDisconnected(object sender, QpServerChannel e)
        {
            var stream = e.GetStream();
            if (stream == null)
                return;
            if (stream is not PipesStream pipesStream)
                return;
            var channelId = pipesStream.ChannelId;
            lock (httpContextDict)
                httpContextDict.Remove(channelId);
        }

        public async Task OnNewConnection(string channelId, ConnectionInfo connectionInfo)
        {
            //如果还没有开始接收，则直接关闭
            if (!isStarted)
                return;
            var connectionInfoStr = $"HTTP:{connectionInfo.RemoteIpAddress}:{connectionInfo.RemotePort}";
            var cts = new CancellationTokenSource();
            var qpHttpContext = new QpHttpContext(options, channelId, connectionInfoStr, cts);
            lock (httpContextDict)
                httpContextDict[channelId] = qpHttpContext;
            lock (httpContextQueue)
                httpContextQueue.Enqueue(qpHttpContext);
            await Task.Delay(-1, cts.Token).ContinueWith(t =>
             {
                 if (LogUtils.LogConnection)
                     LogUtils.Log("[Connection]{0} disconnected.", connectionInfoStr);
             });
        }

        public override void Stop()
        {
            isStarted = false;
            ChannelAuchenticateTimeout -= OnChannelAuchenticateTimeoutOrDisconnected;
            ChannelDisconnected -= OnChannelAuchenticateTimeoutOrDisconnected;
            lock (httpContextQueue)
                httpContextQueue.Clear();
            base.Stop();
        }

        protected override async Task InnerAcceptAsync(CancellationToken token)
        {
            QpHttpContext[] qpHttpContexts = null;
            lock (httpContextQueue)
            {
                qpHttpContexts = httpContextQueue.ToArray();
                httpContextQueue.Clear();
            }
            //如果当前没有HTTP连接，则等待0.1秒后再返回
            if (qpHttpContexts == null || qpHttpContexts.Length == 0)
            {
                await Task.Delay(100).ConfigureAwait(false);
                return;
            }
            foreach (var context in qpHttpContexts)
            {
                try
                {
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]{0} connected.", context.ConnectionInfo);
                    OnNewChannelConnected(context.Stream, context.ConnectionInfo, token, false);
                }
                catch (Exception ex)
                {
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[Connection]Init&Start Channel error,reason:{0}", ex.ToString());
                }
            }
        }

        public async Task HandleRequest(HttpContext context, Func<Task> next)
        {
            if (!isStarted)
            {
                await next().ConfigureAwait(false);
                return;
            }
            var req = context.Request;
            var rep = context.Response;


            if (!req.Headers.ContainsKey(QP_CHANNEL_ID))
            {
                switch (req.Method)
                {
                    case "GET":
                        var qpLibVersion = typeof(QpChannel).Assembly.GetName().Version;
                        var message = $@"
<html>
    <head>
        <title>Quick.Protocol</title>
    </head>
    <body>
        <p>Welcome to use <b>Quick.Protocol {qpLibVersion.ToString(3)}</b></p>
        <p>Source Code:<a href=""https://github.com/QuickProtocol/"">https://github.com/QuickProtocol/</a></p>
        <p>InstructionSet:{string.Join(" | ", options.InstructionSet.Select(t => $"{t.Name}({t.Id})"))}</p>
        <p>ServerProgram:{options.ServerProgram}</p>
        <p>MaxPackageSize:{options.MaxPackageSize}</p>
        <p>HeartBeatInterval:{options.HeartBeatInterval}</p>
        <p>Time:{DateTime.Now}</p>
    </body>
</html>";

                        rep.ContentType = "text/html; charset=utf-8";
                        rep.ContentLength = Encoding.UTF8.GetByteCount(message);
                        await rep.WriteAsync(message, Encoding.UTF8).ConfigureAwait(false);
                        return;
                    case "POST":
                        var newChannelId = Guid.NewGuid().ToString("N");
                        _ = OnNewConnection(newChannelId, context.Connection);
                        rep.ContentType = "text/plain; charset=utf-8";
                        rep.ContentLength = Encoding.UTF8.GetByteCount(newChannelId);
                        await rep.WriteAsync(newChannelId, Encoding.UTF8).ConfigureAwait(false);
                        return;
                    default:
                        await next().ConfigureAwait(false);
                        return;
                }
            }
            var channelId = req.Headers[QP_CHANNEL_ID].ToString();
            QpHttpContext httpContext = null;
            lock (httpContextDict)
                httpContextDict.TryGetValue(channelId, out httpContext);
            if (httpContext == null)
            {
                rep.StatusCode = 401;
                await rep.WriteAsync($"Unknown channel: {channelId}");
                return;
            }
            switch (req.Method)
            {
                case "GET":
                    await httpContext.OnGetData(rep);
                    return;
                case "POST":
                    await httpContext.OnDataRecvAsync(req.Body);
                    rep.StatusCode = 200;
                    rep.ContentLength = 0;
                    await rep.CompleteAsync();
                    return;
            }
        }
    }
}
