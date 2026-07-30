namespace Quick.Protocol;

public class QpStdioClient : QpClient
{
    public QpStdioClient(QpStdioClientOptions options) : base(options) { }

    protected override Task<Stream> InnerConnectAsync()
    {
        Stream stream = new Streams.InputOutputStream(Console.OpenStandardInput(), Console.OpenStandardOutput());
        return Task.FromResult(stream);
    }
}