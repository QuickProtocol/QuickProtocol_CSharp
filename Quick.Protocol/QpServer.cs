using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol
{
    public abstract class QpServer
    {
        private CancellationTokenSource cts;
        private QpServerOptions options;
        private List<QpServerChannel> channelList = new List<QpServerChannel>();
        private List<QpServerChannel> auchenticatedChannelList = new List<QpServerChannel>();

        /// <summary>
        /// 增加Tag属性，用于引用与QpServer相关的对象
        /// </summary>
        public Object Tag { get; set; }

        /// <summary>
        /// 获取全部的通道
        /// </summary>
        public QpServerChannel[] Channels { get; private set; } = new QpServerChannel[0];

        /// <summary>
        /// 已通过认证的通道
        /// </summary>
        public QpServerChannel[] AuchenticatedChannels { get; private set; } = new QpServerChannel[0];

        /// <summary>
        /// 通道连接上时
        /// </summary>
        public event EventHandler<QpServerChannel> ChannelConnected;

        /// <summary>
        /// 通道连接断开时
        /// </summary>
        public event EventHandler<QpServerChannel> ChannelDisconnected;

        public QpServer(QpServerOptions options)
        {
            options.Check();
            this.options = options;
        }

        public virtual void Start()
        {
            cts = new CancellationTokenSource();
            beginAccept(cts.Token);
        }

        internal void RemoveChannel(QpServerChannel channel)
        {
            lock (channelList)
                if (channelList.Contains(channel))
                {
                    channelList.Remove(channel);
                    Channels = channelList.ToArray();
                }
            lock (auchenticatedChannelList)
                if (auchenticatedChannelList.Contains(channel))
                {
                    auchenticatedChannelList.Remove(channel);
                    AuchenticatedChannels = auchenticatedChannelList.ToArray();
                }
        }

        protected void OnNewChannelConnected(Stream stream, string channelName, CancellationToken token)
        {
            var channel = new QpServerChannel(this, stream, channelName, token, options.Clone());
            //将通道加入到全部通道列表里面
            lock (channelList)
            {
                channelList.Add(channel);
                Channels = channelList.ToArray();
            }

            //认证通过后，才将通道添加到已认证通道列表里面
            channel.Auchenticated += (sender, e) =>
            {
                lock (auchenticatedChannelList)
                {
                    auchenticatedChannelList.Add(channel);
                    AuchenticatedChannels = auchenticatedChannelList.ToArray();
                }
            };
            channel.Disconnected += (sender, e) =>
            {
                if (LogUtils.LogConnection)
                    LogUtils.Log("[Connection]{0} Disconnected.", channelName);
                RemoveChannel(channel);
                try { stream.Dispose(); }
                catch { }
                ChannelDisconnected?.Invoke(this, channel);
            };
            Task.Run(() =>
            {
                ChannelConnected?.Invoke(this, channel);
            });
        }

        protected abstract Task InnerAcceptAsync(CancellationToken token);

        private void beginAccept(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            InnerAcceptAsync(token).ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                if (task.IsFaulted)
                    return;
                beginAccept(token);
            });
        }

        public virtual void Stop()
        {
            cts.Cancel();
            cts = null;
        }
    }
}
