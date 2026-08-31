using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Http.Client;

public class HttpClientsStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => throw new NotImplementedException();
    public override bool CanWrite => true;
    public override long Length => throw new NotImplementedException();
    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
    public override void SetLength(long value) => throw new NotImplementedException();
    public override void Flush() { }

    private CancellationTokenSource cts;
    private HttpClient recvClient;
    private HttpClient sendClient;
    private string url;
    private Pipe recvPipe;

    public HttpClientsStream(HttpClient recvClient, HttpClient sendClient, string url)
    {
        this.recvClient = recvClient;
        this.sendClient = sendClient;
        this.url = url;

        cts = new();
        recvPipe = new();
        _ = beginRecv(recvPipe.Writer, cts.Token);
    }

    private async Task beginRecv(PipeWriter writer, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var stream = await recvClient.GetStreamAsync(url, cancellationToken))
                using (var pipeStream = writer.AsStream(true))
                    await stream.CopyToAsync(pipeStream, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 不支持同步读取。底层是 PipeReader + HttpClient，均只有异步 API，
    /// 以 .Result 阻塞等待会在带 SynchronizationContext 的线程（UI / 老式 ASP.NET）上死锁。
    /// 本流的使用方（QpChannel 收发循环）全部走异步重载。
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException($"{nameof(HttpClientsStream)} 不支持同步读取，请使用 ReadAsync。");

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var readRet = await recvPipe.Reader.ReadAsync(cancellationToken);
        if (readRet.Buffer.IsEmpty)
            return 0;
        var ret = Math.Min((int)readRet.Buffer.Length, count);
        var srcBuffer = readRet.Buffer.Slice(0, ret);
        srcBuffer.CopyTo(new Span<byte>(buffer, offset, ret));
        recvPipe.Reader.AdvanceTo(readRet.Buffer.GetPosition(ret));
        return ret;
    }

    /// <summary>
    /// 不支持同步写入。理由同 <see cref="Read(byte[], int, int)"/>：HttpClient 无同步 API，
    /// 以 .Result 阻塞等待会在带 SynchronizationContext 的线程上死锁。
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException($"{nameof(HttpClientsStream)} 不支持同步写入，请使用 WriteAsync。");

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var httpContent = new ByteArrayContent(buffer, offset, count);
        var rep = await sendClient.PostAsync(url, httpContent, cancellationToken);
        if (!rep.IsSuccessStatusCode)
            throw new IOException($"{rep.StatusCode} {rep.ReasonPhrase}");
    }

    protected override void Dispose(bool disposing)
    {
        cts?.Cancel();
        cts?.Dispose();
        recvPipe.Reader.Complete();
        recvPipe.Writer.Complete();
        recvClient.Dispose();
        sendClient.Dispose();
        base.Dispose(disposing);
    }
}
