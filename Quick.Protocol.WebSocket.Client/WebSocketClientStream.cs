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

        private System.Net.WebSockets.ClientWebSocket client;
        private string closeReason = null;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }


        public WebSocketClientStream(System.Net.WebSockets.ClientWebSocket client)
        {
            this.client = client;
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (closeReason != null)
                throw new IOException(closeReason);
            var result = client.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), CancellationToken.None).Result;
            return result.Count;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), CancellationToken.None)
                .ConfigureAwait(false);
            return result.Count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (closeReason != null)
                throw new IOException(closeReason);

            client.SendAsync(
                new ArraySegment<byte>(buffer, offset, count),
                System.Net.WebSockets.WebSocketMessageType.Binary,
                true,
                CancellationToken.None).Wait();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return client.SendAsync(new ArraySegment<byte>(buffer, offset, count), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        protected override void Dispose(bool disposing)
        {
            client.Dispose();
            base.Dispose(disposing);
        }
    }
}
