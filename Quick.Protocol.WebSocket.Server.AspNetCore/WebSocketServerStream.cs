using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.WebSocket.Server.AspNetCore
{
    internal class WebSocketServerStream : Stream
    {
        private System.Net.WebSockets.WebSocket webSocket;
        private CancellationTokenSource cts = null;
        private const int ReadSize = 1024 * 4;
        private byte[] readBuffer = new byte[ReadSize];

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

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                var result = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cts.Token).Result;
                return result.Count;
            }
            catch
            {
                cts.Cancel();
                return 0;
            }
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, offset, count),
                    System.Net.WebSockets.WebSocketMessageType.Binary,
                    true,
                    cts.Token)
                    .Wait();
            }
            catch
            {
                cts.Cancel();
            }
        }

        protected override void Dispose(bool disposing)
        {
            cts.Cancel();
            webSocket.Dispose();
            base.Dispose(disposing);
        }
    }
}
