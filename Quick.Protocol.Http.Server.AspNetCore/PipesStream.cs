using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Http.Server.AspNetCore
{
    internal class PipesStream : Stream
    {
        private Pipe readPipe;
        private Pipe writePipe;

        public PipesStream(Pipe readPipe, Pipe writePipe)
        {
            this.readPipe = readPipe;
            this.writePipe = writePipe;
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
            var reader = readPipe.Reader;
            var readResult = reader.ReadAsync().Result;
            var ret = Math.Min((int)readResult.Buffer.Length, count);
            var srcBuffer = readResult.Buffer.Slice(0, ret);
            srcBuffer.CopyTo(new Span<byte>(buffer, offset, ret));
            reader.AdvanceTo(srcBuffer.GetPosition(ret));
            return ret;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var reader = readPipe.Reader;
            var readResult = await reader.ReadAsync(cancellationToken);
            var ret = Math.Min((int)readResult.Buffer.Length, count);
            var srcBuffer = readResult.Buffer.Slice(0, ret);
            srcBuffer.CopyTo(new Span<byte>(buffer, offset, ret));
            reader.AdvanceTo(srcBuffer.GetPosition(ret));
            return ret;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            using (var stream = writePipe.Writer.AsStream(true))
                stream.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            using (var stream = writePipe.Writer.AsStream(true))
                await stream.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }
}
