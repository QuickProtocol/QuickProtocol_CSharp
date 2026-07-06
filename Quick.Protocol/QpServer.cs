namespace Quick.Protocol;

public abstract class QpServer
{
    private CancellationTokenSource cts;
    public QpServerOptions Options { get; private set; }

    private List<QpServerChannel> channelList = new List<QpServerChannel>();

    /// <summary>
    /// 增加Tag属性，用于引用与QpServer相关的对象
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// 已通过认证的通道
    /// </summary>
    public QpServerChannel[] Channels { get; private set; } = [];

    /// <summary>
    /// 通道连接上时
    /// </summary>
    public event EventHandler<QpServerChannel> ChannelConnected;
    /// <summary>
    /// 通道认证通过
    /// </summary>
    public event EventHandler<QpServerChannel> ChannelAuchenticated;
    /// <summary>
    /// 通道连接断开时
    /// </summary>
    public event EventHandler<QpServerChannel> ChannelDisconnected;

    public QpServer(QpServerOptions options)
    {
        options.Check();
        Options = options;
    }

    public virtual void Start()
    {
        cts = new CancellationTokenSource();
        _ = beginAccept(cts.Token);
    }

    internal void RemoveChannel(QpServerChannel channel)
    {
        lock (channelList)
            if (channelList.Contains(channel))
            {
                channelList.Remove(channel);
                Channels = channelList.ToArray();
            }
    }

    protected void OnNewChannelConnected(Stream stream, string channelName, CancellationToken token, bool readFromStreamReturnZeroMeansFault = true)
    {
        var channel = new QpServerChannel(stream, channelName, token, Options.Clone(), readFromStreamReturnZeroMeansFault);
        ChannelConnected?.Invoke(this, channel);
        //认证通过后，才将通道添加到已连接通道列表里面
        channel.Auchenticated += channel_Auchenticated;
        //断开连接时
        channel.Disconnected += channel_Disconnected;
        channel.Start();
    }

    private void channel_Auchenticated(object sender, EventArgs e)
    {
        var channel = (QpServerChannel)sender;
        lock (channelList)
        {
            channelList.Add(channel);
            Channels = channelList.ToArray();
        }
        ChannelAuchenticated?.Invoke(this, channel);
    }
    private void channel_Disconnected(object sender, EventArgs e)
    {
        var channel = (QpServerChannel)sender;
        channel.Auchenticated -= channel_Auchenticated;
        channel.Disconnected -= channel_Disconnected;

        if (Options.Logger is { LogConnection: true })
            Options.Logger.Log("{0} Disconnected.", channel.ChannelName);
        RemoveChannel(channel);
        ChannelDisconnected?.Invoke(this, channel);
    }

    protected abstract Task InnerAcceptAsync(CancellationToken token);

    private async Task beginAccept(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await InnerAcceptAsync(token);
        }
    }

    public virtual void Stop()
    {
        cts?.Cancel();
        cts = null;

        QpServerChannel[] channels;
        lock (channelList)
        {
            channels = channelList.ToArray();
            channelList.Clear();
            Channels = Array.Empty<QpServerChannel>();
        }
        foreach (var channel in channels)
            try { channel.Disconnect(); }
            catch { }
    }
}