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

        /// <summary>
        /// 不支持同步读取。底层是 PipeReader（只有异步 API），以 .Result 阻塞等待会阻塞
        /// ASP.NET Core 请求线程（线程池饥饿），且在管道背压下可能死锁。
        /// 本流的使用方（QpChannel 收发循环）全部走异步重载。
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException($"{nameof(PipesStream)} 不支持同步读取，请使用 ReadAsync。");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

        /// <summary>
        /// 直接把管道数据拷入调用方内存，避免基类默认实现的 byte[] 中转，
        /// 且返回 ValueTask（数据已缓冲、同步完成时零分配）。
        /// </summary>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var reader = readPipe.Reader;
            var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var ret = Math.Min((int)readResult.Buffer.Length, buffer.Length);
            var srcBuffer = readResult.Buffer.Slice(0, ret);
            srcBuffer.CopyTo(buffer.Span);
            reader.AdvanceTo(srcBuffer.GetPosition(ret));
            return ret;
        }

        /// <summary>
        /// 不支持同步写入。理由同 <see cref="Read(byte[], int, int)"/>：
        /// FlushAsync 在管道触发背压时会挂起，以 .Result 阻塞等待可能死锁。
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException($"{nameof(PipesStream)} 不支持同步写入，请使用 WriteAsync。");

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

        /// <summary>
        /// 直接从调用方内存拷入管道，避免基类默认实现的 byte[] 中转。
        /// </summary>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var writer = writePipe.Writer;
            var memory = writer.GetMemory(buffer.Length);
            buffer.Span.CopyTo(memory.Span);
            writer.Advance(buffer.Length);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
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
