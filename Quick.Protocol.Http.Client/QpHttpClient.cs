using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Http.Client;

[DisplayName("Http")]
public class QpHttpClient : QpClient
{
    protected override bool ReadFromStreamReturnZeroMeansFault => false;
    public const string QP_CHANNEL_ID = nameof(QP_CHANNEL_ID);
    private QpHttpClientOptions options;
    private HttpClient sendClient;
    private HttpClient recvClient;

    public QpHttpClient(QpHttpClientOptions options) : base(options)
    {
        this.options = options;
    }

    protected override async Task<Stream> InnerConnectAsync()
    {
        recvClient = new() { Timeout = TimeSpan.FromMilliseconds(options.HttpClientTimeout) };
        var url = options.Url;
        if (url.StartsWith("qp."))
            url = url.Substring(3);
        var cts = new CancellationTokenSource();
        try
        {
            var rep = await recvClient.PostAsync(url, null);
            if (!rep.IsSuccessStatusCode)
                throw new IOException($"{rep.StatusCode} {rep.ReasonPhrase}");
            var channelId = await rep.Content.ReadAsStringAsync();
            recvClient.DefaultRequestHeaders.Add(QP_CHANNEL_ID, channelId);
            sendClient = new() { Timeout = TimeSpan.FromMilliseconds(options.HttpClientTimeout) };
            sendClient.DefaultRequestHeaders.Add(QP_CHANNEL_ID, channelId);
        }
        catch
        {
            cts.Cancel();
            recvClient.Dispose();
            throw;
        }
        return new HttpClientsStream(recvClient, sendClient, url);
    }
}
