using System.Buffers;

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
            return Task.FromResult(ReadInternal(new Span<byte>(buffer, offset, count)));
        }

        public override int Read(Span<byte> buffer)
        {
            return ReadInternal(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new ValueTask<int>(ReadInternal(buffer.Span));
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                while (_sequence.Length > 0)
                {
                    var count = Math.Min((int)_sequence.Length, bufferSize);
                    _sequence.Slice(0, count).CopyTo(buffer);
                    _sequence = _sequence.Slice(count);
                    await destination.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                }
                await destination.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public override void Flush()
        {
        }
    }
}
