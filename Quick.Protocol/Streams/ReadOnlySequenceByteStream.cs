using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Streams
{
    public class ReadOnlySequenceByteStream : Stream
    {
        private ReadOnlySequence<byte> _sequence;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _sequence.Length;

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

        
        public ReadOnlySequenceByteStream(ReadOnlySequence<byte> sequence)
        {
            _sequence = sequence;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return ReadInternal(new Span<byte>(buffer, offset, count));
        }

        public override int ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            if (ReadInternal(buffer) != 0)
            {
                return buffer[0];
            }

            return -1;
        }

        private int ReadInternal(Span<byte> buffer)
        {
            var count = Math.Min((int)_sequence.Length, buffer.Length);
            _sequence.Slice(0, count).CopyTo(buffer);
            _sequence = _sequence.Slice(count);
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            return Task.Run(() => ReadInternal(new Span<byte>(buffer, offset, count)));
        }

        public override int Read(Span<byte> buffer)
        {
            return ReadInternal(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new ValueTask<int>(Task.Run(() => ReadInternal(buffer.Span)));
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var buffer = new byte[bufferSize];
            while (_sequence.Length > 0)
            {
                var count = Math.Min((int)_sequence.Length, bufferSize);
                _sequence.Slice(0, count).CopyTo(buffer);
                _sequence = _sequence.Slice(count);
                await destination.WriteAsync(buffer, 0, count).ConfigureAwait(false);
            }
            await destination.FlushAsync().ConfigureAwait(false);
        }

        public override void Flush()
        {
        }
    }
}
