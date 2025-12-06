namespace Quick.Protocol.Streams;

public class QpStreamServerChannel : QpServerChannel
{
    public QpStreamServerChannel(QpStreamServerOptions options)
        : base(options.BaseStream, options.ChannelName, options.CancellationToken, options)
    {
    }
}

