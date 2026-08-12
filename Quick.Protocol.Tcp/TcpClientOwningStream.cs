using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Tcp;

/// <summary>
/// Stream wrapper that takes ownership of a TcpClient, disposing it when the stream is disposed.
/// </summary>
internal sealed class TcpClientOwningStream : Stream
{
    private readonly NetworkStream _inner;
    private readonly TcpClient _tcpClient;

    public TcpClientOwningStream(NetworkStream inner, TcpClient tcpClient)
    {
        _inner = inner;
        _tcpClient = tcpClient;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override bool CanTimeout => _inner.CanTimeout;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override int ReadTimeout { get => _inner.ReadTimeout; set => _inner.ReadTimeout = value; }
    public override int WriteTimeout { get => _inner.WriteTimeout; set => _inner.WriteTimeout = value; }

    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override int Read(Span<byte> buffer) => _inner.Read(buffer);
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _inner.ReadAsync(buffer, offset, count, cancellationToken);
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _inner.ReadAsync(buffer, cancellationToken);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
    public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _inner.WriteAsync(buffer, offset, count, cancellationToken);
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _inner.WriteAsync(buffer, cancellationToken);
    public override void Flush() => _inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _inner.BeginRead(buffer, offset, count, callback, state);
    public override int EndRead(IAsyncResult asyncResult) => _inner.EndRead(asyncResult);
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _inner.BeginWrite(buffer, offset, count, callback, state);
    public override void EndWrite(IAsyncResult asyncResult) => _inner.EndWrite(asyncResult);
    public override ValueTask DisposeAsync()
    {
        _inner.Dispose();
        _tcpClient.Dispose();
        return base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            _tcpClient.Dispose();
        }
        base.Dispose(disposing);
    }
}
