using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Udp
{
    internal class UdpClientStream : Stream
    {
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private byte[] firstRecvBuffer;
        private int firstRecvBufferOffset = 0;

        public override bool CanRead => udpClient.Available > 0;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public UdpClientStream(UdpClient udpClient, IPEndPoint remoteEndPoint, byte[] firstRecvBuffer = null)
        {
            this.udpClient = udpClient;
            this.remoteEndPoint = remoteEndPoint;
            this.firstRecvBuffer = firstRecvBuffer;
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).Result;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var receivedBytes = 0;
            if (firstRecvBuffer != null)
            {
                var copyCount = Math.Min(count, firstRecvBuffer.Length - firstRecvBufferOffset);
                for (var i = 0; i < copyCount; i++)
                    buffer[offset + i] = firstRecvBuffer[firstRecvBufferOffset + i];

                offset += copyCount;
                firstRecvBufferOffset += copyCount;
                count -= copyCount;
                receivedBytes = copyCount;
                if (firstRecvBuffer.Length - firstRecvBufferOffset <= 0)
                {
                    firstRecvBuffer = null;
                }
                if (count <= 0)
                    return copyCount;
            }
            var ret = await udpClient.ReceiveAsync(cancellationToken);

            var ret = await udpClient.Client.ReceiveAsync(
                new ArraySegment<byte>(buffer, offset, count),
                SocketFlags.None,
                cancellationToken);
            receivedBytes += ret;
            return receivedBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            udpClient.Send(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await udpClient.SendAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);
        }
    }
}
