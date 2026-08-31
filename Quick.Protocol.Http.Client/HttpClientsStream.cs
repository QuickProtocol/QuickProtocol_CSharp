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

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

    /// <summary>
    /// 直接把管道数据拷入调用方内存，避免基类默认实现的 byte[] 中转，
    /// 且返回 ValueTask（数据已缓冲、同步完成时零分配）。
    /// </summary>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var readRet = await recvPipe.Reader.ReadAsync(cancellationToken);
        //每次 ReadAsync 都必须配对一次 AdvanceTo，否则下一次 ReadAsync 会抛 InvalidOperationException
        if (readRet.Buffer.IsEmpty)
        {
            recvPipe.Reader.AdvanceTo(readRet.Buffer.Start);
            return 0;
        }
        var ret = Math.Min((int)readRet.Buffer.Length, buffer.Length);
        readRet.Buffer.Slice(0, ret).CopyTo(buffer.Span);
        recvPipe.Reader.AdvanceTo(readRet.Buffer.GetPosition(ret));
        return ret;
    }

    /// <summary>
    /// 不支持同步写入。理由同 <see cref="Read(byte[], int, int)"/>：HttpClient 无同步 API，
    /// 以 .Result 阻塞等待会在带 SynchronizationContext 的线程上死锁。
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException($"{nameof(HttpClientsStream)} 不支持同步写入，请使用 WriteAsync。");

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();

    /// <summary>
    /// 用 ReadOnlyMemoryContent 直接包装调用方内存（与 ByteArrayContent 一样不复制数据），
    /// 免去基类默认实现在非数组支撑内存上的租用+拷贝。
    /// </summary>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var httpContent = new ReadOnlyMemoryContent(buffer);
        using (var rep = await sendClient.PostAsync(url, httpContent, cancellationToken))
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
