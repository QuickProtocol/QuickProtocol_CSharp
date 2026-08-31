using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.WebSocket.Client
{
    internal class WebSocketClientStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => throw new NotImplementedException();
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Flush() { }

        private System.Net.WebSockets.ClientWebSocket client;

        public WebSocketClientStream(System.Net.WebSockets.ClientWebSocket client)
        {
            this.client = client;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = client.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), CancellationToken.None).Result;
            return result.Count;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

        /// <summary>
        /// 直接走 ClientWebSocket 的 Memory&lt;byte&gt; 原生 ValueTask 重载，避免一次 ArraySegment 分配，
        /// 且在数据已缓冲（同步完成）时零分配。
        /// </summary>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var result = await client.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            return result.Count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            client.SendAsync(
                new ArraySegment<byte>(buffer, offset, count),
                System.Net.WebSockets.WebSocketMessageType.Binary,
                true,
                CancellationToken.None).Wait();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

        /// <summary>
        /// 直接走 ClientWebSocket 的 ReadOnlyMemory&lt;byte&gt; 原生 ValueTask 重载，避免一次 ArraySegment 分配。
        /// </summary>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => client.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            client.Dispose();
            base.Dispose(disposing);
        }
    }
}
