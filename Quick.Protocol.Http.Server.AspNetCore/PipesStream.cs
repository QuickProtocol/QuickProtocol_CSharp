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
        public string ChannelId { get; private set; }
        private Pipe readPipe;
        private Pipe writePipe;

        public PipesStream(string channelId, Pipe readPipe, Pipe writePipe)
        {
            ChannelId = channelId;
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
            var memory = writePipe.Writer.GetMemory(count);
            new ReadOnlySpan<byte>(buffer, offset, count).CopyTo(memory.Span);
            writePipe.Writer.Advance(count);
            _ = writePipe.Writer.FlushAsync().Result;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var memory = writePipe.Writer.GetMemory(count);
            new ReadOnlySpan<byte>(buffer, offset, count).CopyTo(memory.Span);
            writePipe.Writer.Advance(count);
            await writePipe.Writer.FlushAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { readPipe.Reader.Complete(); } catch { }
                try { readPipe.Writer.Complete(); } catch { }
                try { writePipe.Reader.Complete(); } catch { }
                try { writePipe.Writer.Complete(); } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
