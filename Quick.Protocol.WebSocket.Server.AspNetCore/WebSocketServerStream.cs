using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.WebSocket.Server.AspNetCore
{
    internal class WebSocketServerStream : Stream
    {
        private System.Net.WebSockets.WebSocket webSocket;
        private CancellationTokenSource cts;

        public WebSocketServerStream(System.Net.WebSockets.WebSocket webSocket, CancellationTokenSource cts)
        {
            this.webSocket = webSocket;
            this.cts = cts;
        }

        public override bool CanSeek => throw new NotImplementedException();
        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void SetLength(long value) { throw new NotImplementedException(); }

        public override long Length => throw new NotImplementedException();
        public override long Position { get; set; }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override void Flush() { }

        /// <summary>
        /// 不支持同步读取。WebSocket 仅提供异步 API，以 .Result 阻塞等待会阻塞 ASP.NET Core
        /// 请求线程（线程池饥饿），在带 SynchronizationContext 的宿主上还会死锁。
        /// 本流的使用方（QpChannel 收发循环）全部走异步重载。
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException($"{nameof(WebSocketServerStream)} 不支持同步读取，请使用 ReadAsync。");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

        /// <summary>
        /// 直接走 WebSocket 的 Memory&lt;byte&gt; 原生 ValueTask 重载，避免一次 ArraySegment 分配，
        /// 且在数据已缓冲（同步完成）时零分配。
        /// </summary>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            return result.Count;
        }

        /// <summary>
        /// 不支持同步写入。理由同 <see cref="Read(byte[], int, int)"/>。
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException($"{nameof(WebSocketServerStream)} 不支持同步写入，请使用 WriteAsync。");

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

        /// <summary>
        /// 直接走 WebSocket 的 ReadOnlyMemory&lt;byte&gt; 原生 ValueTask 重载，避免一次 ArraySegment 分配。
        /// </summary>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => webSocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, cancellationToken);


        protected override void Dispose(bool disposing)
        {
            cts.Cancel();
            cts.Dispose();
            webSocket.Dispose();
            base.Dispose(disposing);
        }
    }
}
