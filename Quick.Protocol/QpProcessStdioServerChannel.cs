namespace Quick.Protocol;

public class QpProcessStdioServerChannel : QpServerChannel
{
    public QpProcessStdioServerChannel(QpProcessStdioServerOptions options)
        : base(options.GetStream(), options.ChannelName, options.CancellationToken, options)
    {
        Start();
    }
}

