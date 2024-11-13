using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Streams
{
    internal sealed class PipeWriterStream : Stream
    {
        private readonly PipeWriter _pipeWriter;

        internal bool LeaveOpen { get; set; }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;
        private long _Length = 0;
        public override long Length => _Length;

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public PipeWriterStream(PipeWriter pipeWriter, bool leaveOpen)
        {
            _pipeWriter = pipeWriter;
            LeaveOpen = leaveOpen;
        }

        protected override void Dispose(bool disposing)
        {
            if (!LeaveOpen)
            {
                _pipeWriter.Complete();
            }
        }

        public override ValueTask DisposeAsync()
        {
            if (!LeaveOpen)
            {
                return _pipeWriter.CompleteAsync();
            }

            return default(ValueTask);
        }

        public override void Flush()
        {
            FlushAsync().GetAwaiter().GetResult();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public sealed override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            ValueTask<FlushResult> valueTask = _pipeWriter.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);
            _Length += count;
            return GetFlushResultAsTask(valueTask);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValueTask<FlushResult> valueTask = _pipeWriter.WriteAsync(buffer, cancellationToken);
            _Length += buffer.Length;
            return new ValueTask(GetFlushResultAsTask(valueTask));
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ValueTask<FlushResult> valueTask = _pipeWriter.FlushAsync(cancellationToken);
            return GetFlushResultAsTask(valueTask);
        }

        private static Task GetFlushResultAsTask(ValueTask<FlushResult> valueTask)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                if (valueTask.Result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                return Task.CompletedTask;
            }

            return AwaitTask(valueTask);
            static async Task AwaitTask(ValueTask<FlushResult> valueTask)
            {
                if ((await valueTask.ConfigureAwait(continueOnCapturedContext: false)).IsCanceled)
                {
                    throw new OperationCanceledException();
                }
            }
        }
    }
}
