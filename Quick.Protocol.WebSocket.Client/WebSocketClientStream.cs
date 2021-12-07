using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private byte[] buffer = new byte[1024];
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (closeReason != null)
                throw new IOException(closeReason);
            client.SendAsync(new ArraySegment<byte>(buffer, offset, count), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        protected override void Dispose(bool disposing)
        {
            client.Dispose();
            base.Dispose(disposing);
        }
    }
}
