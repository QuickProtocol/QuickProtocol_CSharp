namespace Quick.Protocol
{
    public abstract class QpServer : IDisposable
    {
        private CancellationTokenSource cts;
        public QpServerOptions Options { get; private set; }
        public abstract string BindingPath { get; }
        
        private List<QpServerChannel> channelList = new List<QpServerChannel>();

        /// <summary>
        /// 增加Tag属性，用于引用与QpServer相关的对象
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 已通过认证的通道
        /// </summary>
        public QpServerChannel[] Channels { get; private set; } = new QpServerChannel[0];

        /// <summary>
        /// 通道正在连接(物理连接建立)
        /// </summary>
        public event EventHandler<QpServerChannel> ChannelConnecting;
        /// <summary>
        /// 通道连接上时(通过认证)
        /// </summary>
        public event EventHandler<QpServerChannel> ChannelConnected;

        /// <summary>
        /// 通道连接断开时(通过认证的通道才会触发)
        /// </summary>
        public event EventHandler<QpServerChannel> ChannelDisconnected;

        /// <summary>
        /// 通道认证超时
        /// </summary>
        public event EventHandler<QpServerChannel> ChannelAuthenticateTimeout;

        public QpServer(QpServerOptions options)
        {
            options.Check();
            Options = options;
        }

        public virtual void Start()
        {
            cts?.Cancel();
            cts?.Dispose();
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
            var channel = new QpServerChannel(stream, channelName, token, Options, readFromStreamReturnZeroMeansFault);
            ChannelConnecting?.Invoke(this, channel);

            //认证超时
            channel.AuthenticateTimeout += (_, _) =>
            {
                if (Options.Logger is { LogConnection: true })
                    Options.Logger.Log("{0} Authenticate timeout.", channelName);
                ChannelAuthenticateTimeout?.Invoke(this, channel);
            };

            //认证通过后，才将通道添加到已连接通道列表里面
            channel.Authenticated += (_, _) =>
            {
                lock (channelList)
                {
                    channelList.Add(channel);
                    Channels = channelList.ToArray();
                }
                ChannelConnected?.Invoke(this, channel);
                channel.Disconnected += (_, _) =>
                {
                    if (Options.Logger is { LogConnection: true })
                        Options.Logger.Log("{0} Disconnected.", channelName);
                    RemoveChannel(channel);
                    ChannelDisconnected?.Invoke(this, channel);
                };
            };
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
            cts?.Dispose();
            cts = null;

            QpServerChannel[] channels;
            lock (channelList)
            {
                channels = channelList.ToArray();
                channelList.Clear();
                Channels = Array.Empty<QpServerChannel>();
            }
            foreach (var channel in channels)
            {
                try { channel.Disconnect(); }
                catch { }
                try { channel.Dispose(); }
                catch { }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
