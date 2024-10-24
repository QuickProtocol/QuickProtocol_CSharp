using System.Text.Json;
using Quick.Protocol.Exceptions;
using Quick.Protocol.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.IO.Pipelines;
using System.Buffers;
using System.Collections.ObjectModel;
using Nerdbank.Streams;

namespace Quick.Protocol
{
    public abstract partial class QpChannel
    {
        /// <summary>
        /// 包长度字节长度
        /// </summary>
        public const int PACKAGE_TOTAL_LENGTH_LENGTH = 4;
        /// <summary>
        /// 包头长度
        /// </summary>
        public const int PACKAGE_HEAD_LENGTH = 5;
        /// <summary>
        /// 命令编号长度(字节数)
        /// </summary>
        public const int COMMAND_ID_LENGTH = 16;

        /// <summary>
        /// 心跳包
        /// </summary>
        private static readonly byte[] HEARTBEAT_PACKAGHE = new byte[] { 0, 0, 0, 5, 0 };
        private const int minimumBufferSize = 1024;
        
        private Stream QpPackageHandler_Stream;
        private readonly QpChannelOptions options;
        private DateTime lastSendPackageTime = DateTime.MinValue;

        private readonly byte[] passwordMd5Buffer;
        private readonly ICryptoTransform enc;
        private readonly ICryptoTransform dec;
        private readonly Encoding encoding = Encoding.UTF8;

        //发送包锁对象
        private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
        //断开连接锁对象
        private readonly object DISCONNECT_LOCK_OBJ = new object();

        private readonly Dictionary<Type, IQpSerializer> typeSerializerDict = new Dictionary<Type, IQpSerializer>();
        private readonly Dictionary<string, Type> commandRequestTypeDict = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> commandResponseTypeDict = new Dictionary<string, Type>();
        private readonly Dictionary<Type, Type> commandRequestTypeResponseTypeDict = new Dictionary<Type, Type>();

        private readonly ConcurrentDictionary<string, CommandContext> commandDict = new ConcurrentDictionary<string, CommandContext>();

        private bool _IsConnected = false;
        /// <summary>
        /// 当前是否连接，要连接且认证通过后，才设置此属性为true
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
            protected set
            {
                _IsConnected = value;
                if (value)
                    LastConnectedTime = DateTime.Now;
                else
                    LastDisconnectedTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 通道名称
        /// </summary>
        public abstract string ChannelName { get; }
        /// <summary>
        /// 认证问题
        /// </summary>
        public string AuthenticateQuestion { get; protected set; }

        //长整型数字的一半，统计大于这个数时，统计计数归零，防止溢出
        private readonly long LONG_HALF_MAX_VALUE = long.MaxValue / 2;
        /// <summary>
        /// 总共接收到的字节数量
        /// </summary>
        public long BytesReceived { get; private set; }
        /// <summary>
        /// 总共发送的字节数量
        /// </summary>
        public long BytesSent { get; private set; }
        /// <summary>
        /// 每秒接收到的字节数量
        /// </summary>
        public long BytesReceivedPerSec { get; private set; }
        /// <summary>
        /// 每秒发送的字节数量
        /// </summary>
        public long BytesSentPerSec { get; private set; }
        /// <summary>
        /// 包发送队列数量
        /// </summary>
        public int PackageSendQueueCount = 0;

        /// <summary>
        /// 最后一次连接的时间
        /// </summary>
        public DateTime? LastConnectedTime { get; private set; }
        /// <summary>
        /// 最后一次断开的时间
        /// </summary>
        public DateTime? LastDisconnectedTime { get; private set; }
        /// <summary>
        /// 连接断开时
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// 断开连接时
        /// </summary>
        public virtual void Disconnect()
        {
            var shouldRaiseDisconnectedEvent = false;
            lock (DISCONNECT_LOCK_OBJ)
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    shouldRaiseDisconnectedEvent = true;
                }
            }
            InitQpPackageHandler_Stream(null);
            if (shouldRaiseDisconnectedEvent)
                Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 最后的异常
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        /// 收到心跳数据包事件
        /// </summary>
        public event EventHandler HeartbeatPackageReceived;
        /// <summary>
        /// 原始收到通知数据包事件
        /// </summary>
        public event EventHandler<RawNoticePackageReceivedEventArgs> RawNoticePackageReceived;
        /// <summary>
        /// 收到通知数据包事件
        /// </summary>
        public event EventHandler<NoticePackageReceivedEventArgs> NoticePackageReceived;
        /// <summary>
        /// 原始收到命令请求数据包事件
        /// </summary>
        public event EventHandler<RawCommandRequestPackageReceivedEventArgs> RawCommandRequestPackageReceived;
        /// <summary>
        /// 收到命令请求数据包事件
        /// </summary>
        public event EventHandler<CommandRequestPackageReceivedEventArgs> CommandRequestPackageReceived;
        /// <summary>
        /// 收到命令响应数据包事件
        /// </summary>
        public event EventHandler<CommandResponsePackageReceivedEventArgs> CommandResponsePackageReceived;

        protected void ChangeTransportTimeout()
        {
            var stream = QpPackageHandler_Stream;
            if (stream != null && stream.CanTimeout)
            {
                stream.WriteTimeout = options.InternalTransportTimeout;
                stream.ReadTimeout = options.InternalTransportTimeout;
            }
        }

        private IQpSerializer getTypeSerializer(Type type)
        {
            if (typeSerializerDict.TryGetValue(type, out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// 增加Tag属性，用于引用与处理器相关的对象
        /// </summary>
        public object Tag { get; set; }

        private readonly Dictionary<string, Type> noticeTypeDict = new Dictionary<string, Type>();

        public QpChannel(QpChannelOptions options)
        {
            this.options = options;
            passwordMd5Buffer = CryptographyUtils.ComputeMD5Hash(Encoding.UTF8.GetBytes(options.Password)).Take(8).ToArray();

            DES des = DES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            enc = des.CreateEncryptor(passwordMd5Buffer, passwordMd5Buffer);
            dec = des.CreateDecryptor(passwordMd5Buffer, passwordMd5Buffer);

            foreach (var instructionSet in options.InstructionSet)
            {
                //添加通知数据包信息
                if (instructionSet.NoticeInfos != null && instructionSet.NoticeInfos.Length > 0)
                {
                    foreach (var item in instructionSet.NoticeInfos)
                    {
                        noticeTypeDict[item.NoticeTypeName] = item.GetNoticeType();
                        typeSerializerDict[item.GetNoticeType()] = item.GetNoticeSerializer();
                    }
                }
                //添加命令数据包信息
                if (instructionSet.CommandInfos != null && instructionSet.CommandInfos.Length > 0)
                {
                    foreach (var item in instructionSet.CommandInfos)
                    {
                        var requestType = item.GetRequestType();
                        var responseType = item.GetResponseType();
                        commandRequestTypeDict[item.RequestTypeName] = requestType;
                        commandResponseTypeDict[item.ResponseTypeName] = responseType;
                        commandRequestTypeResponseTypeDict[requestType] = responseType;
                        typeSerializerDict[item.GetRequestType()] = item.GetRequestSeriliazer();
                        typeSerializerDict[item.GetResponseType()] = item.GetResponseSeriliazer();
                    }
                }
            }
        }

        protected void InitQpPackageHandler_Stream(Stream stream)
        {
            var preStream = QpPackageHandler_Stream;
            QpPackageHandler_Stream = stream;

            try { preStream?.Dispose(); }
            catch { }

            options.InternalCompress = false;
            options.InternalEncrypt = false;
            ChangeTransportTimeout();
        }


        /// <summary>
        /// 发送通知包
        /// </summary>
        public Task SendNoticePackage(object package)
        {
            var type = package.GetType();
            var serializer = getTypeSerializer(type);
            return SendNoticePackage(type.FullName, serializer.Serialize(package));
        }

        /// <summary>
        /// 发送命令请求包
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task SendCommandRequest(object request)
        {
            var requestType = request.GetType();
            var typeName = requestType.FullName;
            var requestSerializer = getTypeSerializer(requestType);
            var requestContent = requestSerializer.Serialize(request);
            await SendCommandRequestPackage(CommandContext.GenerateNewId(), typeName, requestContent).ConfigureAwait(false);
        }

        protected void BeginHeartBeat(CancellationToken cancellationToken)
        {
            if (options.HeartBeatInterval > 0)
                _ = Task.Delay(options.HeartBeatInterval, cancellationToken).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        return;
                    if (QpPackageHandler_Stream == null)
                        return;

                    var lastSendPackageToNowSeconds = (DateTime.Now - lastSendPackageTime).TotalMilliseconds;

                    //如果离最后一次发送数据包的时间大于心跳间隔，则发送心跳包
                    if (lastSendPackageToNowSeconds > options.HeartBeatInterval)
                    {
                        _ = SendHeartbeatPackage();
                    }
                    BeginHeartBeat(cancellationToken);
                });
        }

        protected void BeginNetstat(CancellationToken cancellationToken)
        {
            if (!options.EnableNetstat)
                return;
            if (cancellationToken.IsCancellationRequested)
                return;

            long preBytesReceived = BytesReceived;
            long preBytesSent = BytesSent;

            _ = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                if (QpPackageHandler_Stream == null)
                    return;
                BytesReceivedPerSec = BytesReceived - preBytesReceived;
                BytesSentPerSec = BytesSent - preBytesSent;
                BeginNetstat(cancellationToken);
            });
        }


        /// <summary>
        /// 添加命令执行器管理器
        /// </summary>
        /// <param name="commandExecuterManager"></param>
        public void AddCommandExecuterManager(CommandExecuterManager commandExecuterManager)
        {
            options.RegisterCommandExecuterManager(commandExecuterManager);
        }

        /// <summary>
        /// 添加通知处理器管理器
        /// </summary>
        /// <param name="noticeHandlerManager"></param>
        public void AddNoticeHandlerManager(NoticeHandlerManager noticeHandlerManager)
        {
            options.RegisterNoticeHandlerManager(noticeHandlerManager);
        }

        public async Task<CommandResponseTypeNameAndContent> SendCommand(string requestTypeName, string requestContent, int timeout = 30 * 1000, Action afterSendHandler = null)
        {
            var commandContext = new CommandContext(requestTypeName);
            commandDict.TryAdd(commandContext.Id, commandContext);

            if (timeout <= 0)
            {
                await SendCommandRequestPackage(commandContext.Id, requestTypeName, requestContent, afterSendHandler).ConfigureAwait(false);
                return await commandContext.ResponseTask.ConfigureAwait(false);
            }
            //如果设置了超时
            else
            {
                try
                {
                    await Task.Run(() => SendCommandRequestPackage(commandContext.Id, requestTypeName, requestContent, afterSendHandler))
                        .WaitAsync(TimeSpan.FromMilliseconds(timeout))
                        .ConfigureAwait(false);
                }
                catch
                {
                    if (LogUtils.LogCommand)
                        LogUtils.Log("{0}: [Send-CommandRequestPackage-Timeout]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandContext.Id, requestTypeName, LogUtils.LogContent ? requestContent : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                    if (commandContext.ResponseTask.Status == TaskStatus.Created)
                    {
                        commandContext.Timeout();
                        commandDict.TryRemove(commandContext.Id, out _);
                    }
                }
                return await commandContext.ResponseTask
                    .WaitAsync(TimeSpan.FromMilliseconds(timeout))
                    .ConfigureAwait(false);
            }
        }

        public async Task<TCmdResponse> SendCommand<TCmdRequest, TCmdResponse>(IQpCommandRequest<TCmdRequest, TCmdResponse> request, int timeout = 30 * 1000, Action afterSendHandler = null)
        {
            var requestType = request.GetType();
            var typeName = requestType.FullName;
            var requestSerializer = getTypeSerializer(requestType);
            var requestContent = requestSerializer.Serialize(request);

            var commandContext = new CommandContext(typeName);
            commandDict.TryAdd(commandContext.Id, commandContext);

            CommandResponseTypeNameAndContent ret = null;
            if (timeout <= 0)
            {
                await SendCommandRequestPackage(commandContext.Id, typeName, requestContent, afterSendHandler).ConfigureAwait(false);
                ret = await commandContext.ResponseTask.ConfigureAwait(false);
            }
            //如果设置了超时
            else
            {
                try
                {
                    await Task.Run(() => SendCommandRequestPackage(commandContext.Id, typeName, requestContent, afterSendHandler))
                        .WaitAsync(TimeSpan.FromMilliseconds(timeout))
                        .ConfigureAwait(false);
                }
                catch
                {
                    if (LogUtils.LogCommand)
                        LogUtils.Log("{0}: [Send-CommandRequestPackage-Timeout]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandContext.Id, typeName, LogUtils.LogContent ? requestContent : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                    if (commandContext.ResponseTask.Status == TaskStatus.Created)
                    {
                        commandContext.Timeout();
                        commandDict.TryRemove(commandContext.Id, out _);
                    }
                }
                ret = await commandContext.ResponseTask
                    .WaitAsync(TimeSpan.FromMilliseconds(timeout))
                    .ConfigureAwait(false);
            }
            var responseType = typeof(TCmdResponse);
            var responseSerializer = getTypeSerializer(responseType);
            return (TCmdResponse)responseSerializer.Deserialize(ret.Content);
        }
    }
}
