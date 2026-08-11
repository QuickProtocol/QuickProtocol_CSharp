using System.Buffers;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;


namespace Quick.Protocol
{
    public abstract partial class QpChannel : IDisposable
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
        private const int minimumBufferSize = 1024;

        private Stream QpPackageHandler_Stream;
        public QpChannelOptions Options { get; }

        private readonly byte[] passwordMd5Buffer;
        private SymmetricAlgorithm symmetricAlgorithm;
        private ICryptoTransform enc;
        private ICryptoTransform dec;
        private readonly Encoding encoding = Encoding.UTF8;

        //发送包锁对象
        private SemaphoreSlim sendLock;
        //断开连接锁对象
        private readonly object DISCONNECT_LOCK_OBJ = new object();

        private List<CommandExecuterManager> commandExecuterManagerList = new();
        private List<NoticeHandlerManager> noticeHandlerManagerList = new();
        private readonly Dictionary<Type, IQpSerializer> typeSerializerDict = new Dictionary<Type, IQpSerializer>();
        private readonly Dictionary<string, Type> commandRequestTypeDict = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> commandResponseTypeDict = new Dictionary<string, Type>();
        private readonly Dictionary<Type, Type> commandRequestTypeResponseTypeDict = new Dictionary<Type, Type>();

        private readonly ConcurrentDictionary<string, CommandContext> commandDict = new ConcurrentDictionary<string, CommandContext>();

        /// <summary>
        /// 是否压缩
        /// </summary>
        public virtual bool EnableCompress { get; protected set; } = false;

        /// <summary>
        /// 是否加密
        /// </summary>
        public virtual bool EnableEncrypt { get; protected set; } = false;

        private string _EncryptAlgorithm;
        /// <summary>
        /// 加密算法
        /// </summary>
        public string EncryptAlgorithm
        {
            get => _EncryptAlgorithm;
            protected set => _EncryptAlgorithm = value?.ToUpper();
        }

        private string _EncryptMode;
        /// <summary>
        /// 加密模式
        /// </summary>
        public string EncryptMode
        {
            get => _EncryptMode;
            protected set => _EncryptMode = value?.ToUpper();
        }
        private string _EncryptPadding;
        /// <summary>
        /// 加密填充
        /// </summary>
        public string EncryptPadding
        {
            get => _EncryptPadding;
            protected set => _EncryptPadding = value?.ToUpper();
        }

        /// <summary>
        /// 接收超时(默认15秒)
        /// </summary>
        public int TransportTimeout { get; protected set; } = 15 * 1000;

        /// <summary>
        /// 心跳间隔，为发送或接收超时中小的值的三分一
        /// </summary>
        public int HeartBeatInterval => TransportTimeout / 3;

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

        public void ClearCommandExecuterManagers()
        {
            lock (commandExecuterManagerList)
                commandExecuterManagerList.Clear();
        }

        public void ClearNoticeHandlerManagers()
        {
            lock (noticeHandlerManagerList)
                noticeHandlerManagerList.Clear();
        }

        // <summary>
        // 注册指令执行器管理器
        // </summary>
        public void RegisterCommandExecuterManagers(IEnumerable<CommandExecuterManager> commandExecuterManagers)
        {
            if (commandExecuterManagers == null)
                return;
            lock (commandExecuterManagerList)
                commandExecuterManagerList.AddRange(commandExecuterManagers);
        }

        /// <summary>
        /// 注册通知处理器管理器
        /// </summary>
        /// <param name="noticeHandlerManager"></param>
        public void RegisterNoticeHandlerManagers(IEnumerable<NoticeHandlerManager> noticeHandlerManagers)
        {
            if (noticeHandlerManagers == null)
                return;
            lock (noticeHandlerManagerList)
                noticeHandlerManagerList.AddRange(noticeHandlerManagers);
        }

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
            ClearCommandExecuterManagers();
            ClearNoticeHandlerManagers();
            enc?.Dispose();
            enc = null;
            dec?.Dispose();
            dec = null;
            symmetricAlgorithm?.Dispose();
            symmetricAlgorithm = null;
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
                stream.WriteTimeout = TransportTimeout;
                stream.ReadTimeout = TransportTimeout;
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
            Options = options;
            passwordMd5Buffer = MD5.HashData(Encoding.UTF8.GetBytes(options.Password)).Take(8).ToArray();

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
                        typeSerializerDict[item.GetRequestType()] = item.GetRequestSerializer();
                        typeSerializerDict[item.GetResponseType()] = item.GetResponseSerializer();
                    }
                }
            }
        }

        protected void OnAuthPassed()
        {
            enc?.Dispose();
            dec?.Dispose();
            symmetricAlgorithm?.Dispose();

            if (EnableEncrypt)
            {
                switch (EncryptAlgorithm)
                {
                    case "DES":
                        symmetricAlgorithm = DES.Create();
                        break;
                    case "AES":
                        symmetricAlgorithm = Aes.Create();
                        break;
                    default:
                        throw new ArgumentException($"Unknown encrypt method: {EncryptAlgorithm}");
                }

                symmetricAlgorithm.Mode = Enum.Parse<CipherMode>(EncryptMode);
                symmetricAlgorithm.Padding = Enum.Parse<PaddingMode>(EncryptPadding);
                enc = symmetricAlgorithm.CreateEncryptor(passwordMd5Buffer, passwordMd5Buffer);
                dec = symmetricAlgorithm.CreateDecryptor(passwordMd5Buffer, passwordMd5Buffer);
            }
        }

        protected void InitQpPackageHandler_Stream(Stream stream)
        {
            var preStream = QpPackageHandler_Stream;
            QpPackageHandler_Stream = stream;

            try { preStream?.Dispose(); }
            catch { }
            if (stream != null && sendLock == null)
                sendLock = new SemaphoreSlim(1, 1);
            EnableCompress = false;
            EnableEncrypt = false;
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

        protected string ComputeMD5Hash(string data)
        {
            int byteCount = encoding.GetByteCount(data);
            byte[] dataBuf = ArrayPool<byte>.Shared.Rent(byteCount);
            var dataSpan = dataBuf.AsSpan(0, byteCount);

            var hashBuffer = ArrayPool<byte>.Shared.Rent(MD5.HashSizeInBytes);
            var hashSpan = hashBuffer.AsSpan(0, MD5.HashSizeInBytes);
            try
            {
                encoding.GetBytes(data, dataSpan);
                MD5.HashData(dataSpan, hashSpan);
                return Convert.ToHexString(hashSpan).ToLower();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(dataBuf);
                ArrayPool<byte>.Shared.Return(hashBuffer);
            }
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

        protected async Task BeginHeartBeat(CancellationToken cancellationToken)
        {
            if (HeartBeatInterval < 0)
                return;
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(HeartBeatInterval, cancellationToken);
                if (QpPackageHandler_Stream == null)
                    return;
                await SendHeartbeatPackage();
            }
        }

        protected async Task BeginNetstat(CancellationToken cancellationToken)
        {
            if (!Options.EnableNetstat)
                return;

            while (!cancellationToken.IsCancellationRequested)
            {
                long preBytesReceived = BytesReceived;
                long preBytesSent = BytesSent;
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                if (QpPackageHandler_Stream == null)
                    return;
                BytesReceivedPerSec = BytesReceived - preBytesReceived;
                BytesSentPerSec = BytesSent - preBytesSent;
            }
        }

        public Task<CommandResponseTypeNameAndContent> SendCommand(string requestTypeName, string requestContent, int timeout = 30 * 1000)
        {
            return SendCommand(requestTypeName, requestContent, timeout, false);
        }

        protected async Task<CommandResponseTypeNameAndContent> SendCommand(string requestTypeName, string requestContent, int timeout, bool ignoreCompressAndEncrypt)
        {
            var commandContext = new CommandContext(requestTypeName);
            commandDict.TryAdd(commandContext.Id, commandContext);

            if (timeout <= 0)
            {
                await SendCommandRequestPackage(commandContext.Id, requestTypeName, requestContent, ignoreCompressAndEncrypt).ConfigureAwait(false);
                return await commandContext.ResponseTask.ConfigureAwait(false);
            }
            //如果设置了超时
            else
            {
                try
                {
                    await SendCommandRequestPackage(commandContext.Id, requestTypeName, requestContent, ignoreCompressAndEncrypt)
                        .WaitAsync(TimeSpan.FromMilliseconds(timeout))
                        .ConfigureAwait(false);
                }
                catch
                {
                    if (Options.Logger is { LogCommand: true })
                        Options.Logger.Log(
                            "{0}: [Send-CommandRequestPackage-Timeout]CommandId:{1},Type:{2},Content:{3}", DateTime.Now,
                            commandContext.Id, requestTypeName,
                            Options.Logger.LogContent ? requestContent : QpLogger.NOT_SHOW_CONTENT_MESSAGE);

                    if (!commandContext.ResponseTask.IsCompleted)
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

        public Task<TCmdResponse> SendCommand<TCmdRequest, TCmdResponse>(IQpCommandRequest<TCmdRequest, TCmdResponse> request, int timeout = 30 * 1000)
        {
            return SendCommand(request, timeout, false);
        }

        protected async Task<TCmdResponse> SendCommand<TCmdRequest, TCmdResponse>(IQpCommandRequest<TCmdRequest, TCmdResponse> request, int timeout, bool ignoreCompressAndEncrypt)
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
                await SendCommandRequestPackage(commandContext.Id, typeName, requestContent, ignoreCompressAndEncrypt).ConfigureAwait(false);
                ret = await commandContext.ResponseTask.ConfigureAwait(false);
            }
            //如果设置了超时
            else
            {
                try
                {
                    await SendCommandRequestPackage(commandContext.Id, typeName, requestContent, ignoreCompressAndEncrypt)
                        .WaitAsync(TimeSpan.FromMilliseconds(timeout))
                        .ConfigureAwait(false);
                }
                catch
                {
                    if (Options.Logger is { LogCommand: true })
                        Options.Logger.Log("{0}: [Send-CommandRequestPackage-Timeout]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandContext.Id, typeName, Options.Logger.LogContent ? requestContent : QpLogger.NOT_SHOW_CONTENT_MESSAGE);

                    if (!commandContext.ResponseTask.IsCompleted)
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

        public void Dispose()
        {
            IsConnected = false;
            Disconnect();
            sendLock?.Dispose();
            sendLock = null;
        }
    }
}
