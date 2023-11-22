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
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol
{
    public abstract class QpChannel
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
        private static byte[] HEARTBEAT_PACKAGHE = new byte[] { 0, 0, 0, 5, 0 };
        private static ArraySegment<byte> nullArraySegment = new ArraySegment<byte>();
        //接收缓存
        private byte[] recvBuffer;
        private byte[] recvBuffer2;
        //发送缓存
        private byte[] sendBuffer;
        private byte[] sendBuffer2;

        private Stream QpPackageHandler_Stream;
        private QpChannelOptions options;
        private DateTime lastSendPackageTime = DateTime.MinValue;

        private byte[] passwordMd5Buffer;
        private ICryptoTransform enc;
        private ICryptoTransform dec;
        private Encoding encoding = Encoding.UTF8;

        private Task sendPackageTask = Task.CompletedTask;
        //发送包锁对象
        private object SEND_PACKAGE_LOCK_OBJ = new object();
        //断开连接锁对象
        private object DISCONNECT_LOCK_OBJ = new object();

        private Dictionary<string, JsonTypeInfo> commandRequestTypeDict = new Dictionary<string, JsonTypeInfo>();
        private Dictionary<string, JsonTypeInfo> commandResponseTypeDict = new Dictionary<string, JsonTypeInfo>();
        private Dictionary<JsonTypeInfo, JsonTypeInfo> commandRequestTypeResponseTypeDict = new Dictionary<JsonTypeInfo, JsonTypeInfo>();

        private ConcurrentDictionary<string, CommandContext> commandDict = new ConcurrentDictionary<string, CommandContext>();

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
        private long LONG_HALF_MAX_VALUE = long.MaxValue / 2;
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

        /// <summary>
        /// 缓存大小，初始大小为1KB
        /// </summary>
        protected int BufferSize { get; set; } = 1 * 1024;

        protected void ChangeBufferSize(int bufferSize)
        {
            //缓存大小最小为1KB
            if (bufferSize < 1 * 1024)
                bufferSize = 1 * 1024;
            BufferSize = bufferSize;
            recvBuffer = new byte[bufferSize];
            recvBuffer2 = new byte[bufferSize];
            sendBuffer = new byte[bufferSize];
            sendBuffer2 = new byte[bufferSize];
        }

        protected void ChangeTransportTimeout()
        {
            var stream = QpPackageHandler_Stream;
            if (stream != null && stream.CanTimeout)
            {
                stream.WriteTimeout = options.InternalTransportTimeout;
                stream.ReadTimeout = options.InternalTransportTimeout;
            }
        }

        /// <summary>
        /// 增加Tag属性，用于引用与处理器相关的对象
        /// </summary>
        public object Tag { get; set; }

        private Dictionary<string, JsonTypeInfo> noticeTypeDict = new Dictionary<string, JsonTypeInfo>();

        public QpChannel(QpChannelOptions options)
        {
            this.options = options;
            ChangeBufferSize(BufferSize);
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
                        noticeTypeDict[item.NoticeTypeName] = item.GetNoticeTypeInfo();
                    }
                }
                //添加命令数据包信息
                if (instructionSet.CommandInfos != null && instructionSet.CommandInfos.Length > 0)
                {
                    foreach (var item in instructionSet.CommandInfos)
                    {
                        var requestType = item.GetRequestTypeInfo();
                        var responseType = item.GetResponseTypeInfo();
                        commandRequestTypeDict[item.RequestTypeName] = requestType;
                        commandResponseTypeDict[item.ResponseTypeName] = responseType;
                        commandRequestTypeResponseTypeDict[requestType] = responseType;
                    }
                }
            }
        }

        private JsonTypeInfo GetNoticeTypeInfo(string typeName)
        {
            if (!noticeTypeDict.TryGetValue(typeName, out var noticeTypeInfo))
                throw new ArgumentException($"Notice type[{typeName}] not found.");
            return noticeTypeInfo;
        }

        private JsonTypeInfo GetCommandRequestTypeInfo(string typeName)
        {
            if (!commandRequestTypeDict.TryGetValue(typeName, out var requestTypeInfo))
                throw new ArgumentException($"Command request type[{typeName}] not found.");
            return requestTypeInfo;
        }

        private JsonTypeInfo GetCommandResponseTypeInfo(string typeName)
        {
            if (!commandResponseTypeDict.TryGetValue(typeName, out var responseTypeInfo))
                throw new ArgumentException($"Command response type[{typeName}] not found.");
            return responseTypeInfo;
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
        /// 当发送出错时
        /// </summary>
        protected virtual void OnWriteError(Exception exception)
        {
            LastException = exception;
            LogUtils.Log("[WriteError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            InitQpPackageHandler_Stream(null);
            Disconnect();
        }

        /// <summary>
        /// 当读取出错时
        /// </summary>
        protected virtual void OnReadError(Exception exception)
        {
            LastException = exception;
            LogUtils.Log("[ReadError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            InitQpPackageHandler_Stream(null);
            Disconnect();
        }

        //获取空闲的缓存
        private byte[] getFreeBuffer(byte[] usingBuffer, params byte[][] bufferArray)
        {
            foreach (var buffer in bufferArray)
            {
                if (usingBuffer != buffer)
                    return buffer;
            }
            return null;
        }

        private async Task writePackageBuffer(Stream stream, ArraySegment<byte> packageBuffer, Action afterSendHandler)
        {
            var packageType = packageBuffer.Array[packageBuffer.Offset + PACKAGE_HEAD_LENGTH - 1];

            //如果压缩或者加密
            if (options.InternalCompress || options.InternalEncrypt)
            {
                //如果压缩
                if (options.InternalCompress)
                {
                    var currentBuffer = getFreeBuffer(packageBuffer.Array, sendBuffer, sendBuffer2);
                    using (var ms = new MemoryStream(currentBuffer))
                    {
                        //写入包长度
                        for (var i = 0; i < PACKAGE_TOTAL_LENGTH_LENGTH; i++)
                            ms.WriteByte(0);
                        using (var gzStream = new GZipStream(ms, CompressionMode.Compress, true))
                            await gzStream.WriteAsync(packageBuffer.Array, packageBuffer.Offset + PACKAGE_TOTAL_LENGTH_LENGTH, packageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH)
                                .ConfigureAwait(false);
                        var packageTotalLength = Convert.ToInt32(ms.Position);
                        writePackageTotalLengthToBuffer(currentBuffer, 0, packageTotalLength);
                        packageBuffer = new ArraySegment<byte>(currentBuffer, 0, packageTotalLength);
                    }
                }
                //如果加密
                if (options.InternalEncrypt)
                {
                    var retBuffer = enc.TransformFinalBlock(packageBuffer.Array, packageBuffer.Offset + PACKAGE_TOTAL_LENGTH_LENGTH, packageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH);
                    var packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + retBuffer.Length;
                    var currentBuffer = getFreeBuffer(packageBuffer.Array, sendBuffer, sendBuffer2);
                    //写入包长度
                    writePackageTotalLengthToBuffer(currentBuffer, 0, packageTotalLength);
                    Array.Copy(retBuffer, 0, currentBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, retBuffer.Length);
                    packageBuffer = new ArraySegment<byte>(currentBuffer, 0, packageTotalLength);
                }
            }

            //执行AfterSendHandler
            afterSendHandler?.Invoke();

            //发送包内容
            var writeTask = stream.WriteAsync(packageBuffer.Array, packageBuffer.Offset, packageBuffer.Count);
            await writeTask
                .WaitAsync(TimeSpan.FromMilliseconds(options.InternalTransportTimeout))
                .ConfigureAwait(false);

            if (writeTask.IsCanceled)
                return;
            if (writeTask.Exception != null)
                throw new IOException("Write error from stream.", writeTask.Exception.InnerException);

            if (options.EnableNetstat)
            {
                BytesSent += packageBuffer.Count;
                if (BytesSent > LONG_HALF_MAX_VALUE)
                    BytesSent = 0;
            }
            if (LogUtils.LogPackage)
                LogUtils.Log(
                    "{0}: [Send-Package]Length:{1}，Type:{2}，Content:{3}",
                    DateTime.Now,
                    packageBuffer.Count,
                    (QpPackageType)packageType,
                    LogUtils.LogContent ?
                        BitConverter.ToString(packageBuffer.Array, packageBuffer.Offset, packageBuffer.Count)
                        : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        private void writePackageTotalLengthToBuffer(byte[] buffer, int offset, int packageTotalLength)
        {
            //构造包头
            var ret = BitConverter.GetBytes(packageTotalLength);
            //如果是小端字节序，则交换
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ret);
            Array.Copy(ret, 0, buffer, offset, sizeof(int));
        }

        private Task writePackageAsync(Func<byte[], ArraySegment<byte>> getPackagePayloadFunc, Action afterSendHandler)
        {
            Interlocked.Increment(ref PackageSendQueueCount);
            lock (SEND_PACKAGE_LOCK_OBJ)
            {
                sendPackageTask = sendPackageTask.ContinueWith(t =>
                {
                    var stream = QpPackageHandler_Stream;
                    if (stream == null)
                        throw new IOException("Connection is disconnected.");
                    writePackage(getPackagePayloadFunc, afterSendHandler).Wait();
                });
                sendPackageTask.ContinueWith(t => Interlocked.Decrement(ref PackageSendQueueCount));
                return sendPackageTask;
            }
        }

        private async Task writePackage(Func<byte[], ArraySegment<byte>> getPackagePayloadFunc, Action afterSendHandler)
        {
            var stream = QpPackageHandler_Stream;
            if (stream == null)
                throw new ArgumentNullException(nameof(QpPackageHandler_Stream));

            var ret = getPackagePayloadFunc(sendBuffer);
            var packageTotalLength = ret.Count;
            if (packageTotalLength < PACKAGE_HEAD_LENGTH)
                throw new IOException($"包大小[{packageTotalLength}]小于包头长度[{PACKAGE_HEAD_LENGTH}]");

            byte[] packageBuffer = ret.Array;

            //构造包头
            writePackageTotalLengthToBuffer(packageBuffer, 0, packageTotalLength);
            try
            {
                //如果包缓存是发送缓存
                if (packageBuffer == sendBuffer)
                {
                    await writePackageBuffer(stream,
                        new ArraySegment<byte>(packageBuffer, 0, packageTotalLength),
                        afterSendHandler).ConfigureAwait(false);
                }
                //否则，拆分为多个包发送
                else
                {
                    if (LogUtils.LogSplit)
                        LogUtils.Log("{0}: [Send-SplitPackage]Length:{1}", DateTime.Now, packageTotalLength);

                    //每个包内容的最大长度为对方缓存大小减包头大小
                    var maxTakeLength = BufferSize - PACKAGE_HEAD_LENGTH;
                    var currentIndex = 0;
                    while (currentIndex < packageTotalLength)
                    {
                        var restLength = packageTotalLength - currentIndex;
                        int takeLength = 0;
                        if (restLength >= maxTakeLength)
                            takeLength = maxTakeLength;
                        else
                            takeLength = restLength;
                        //构造包头
                        writePackageTotalLengthToBuffer(sendBuffer, 0, PACKAGE_HEAD_LENGTH + takeLength);
                        sendBuffer[4] = (byte)QpPackageType.Split;
                        //复制包体
                        Array.Copy(packageBuffer, currentIndex, sendBuffer, PACKAGE_HEAD_LENGTH, takeLength);
                        //发送
                        await writePackageBuffer(
                            stream,
                            new ArraySegment<byte>(sendBuffer, 0, PACKAGE_HEAD_LENGTH + takeLength),
                            afterSendHandler).ConfigureAwait(false);
                        currentIndex += takeLength;
                    }
                }
                lastSendPackageTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                OnWriteError(ex);
                throw;
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        public Task SendHeartbeatPackage()
        {
            return writePackageAsync(buffer =>
            {
                HEARTBEAT_PACKAGHE.CopyTo(buffer, 0);
                return new ArraySegment<byte>(buffer, 0, HEARTBEAT_PACKAGHE.Length);
            }, null);
        }

        public Task SendNoticePackage(string noticePackageTypeName, string noticePackageContent)
        {
            return writePackageAsync(buffer =>
            {
                //设置包类型
                buffer[PACKAGE_HEAD_LENGTH - 1] = (byte)QpPackageType.Notice;
                var typeName = noticePackageTypeName;
                var content = noticePackageContent;

                var typeNameByteLengthOffset = PACKAGE_HEAD_LENGTH;
                //写入类名
                var typeNameByteOffset = typeNameByteLengthOffset + 1;
                var typeNameByteLength = encoding.GetEncoder().GetBytes(typeName.ToCharArray(), 0, typeName.Length, buffer, typeNameByteOffset, true);
                //写入类名长度
                buffer[typeNameByteLengthOffset] = Convert.ToByte(typeNameByteLength);

                var contentOffset = typeNameByteOffset + typeNameByteLength;
                var contentLength = encoding.GetByteCount(content);

                var retBuffer = buffer;
                //如果内容超出了缓存可用空间的大小
                if (contentLength > buffer.Length - contentOffset)
                {
                    retBuffer = new byte[contentOffset + contentLength];
                    Array.Copy(buffer, retBuffer, contentOffset);
                }
                encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, retBuffer, contentOffset, true);

                //包总长度
                var packageTotalLength = contentOffset + contentLength;

                if (LogUtils.LogNotice)
                    LogUtils.Log("{0}: [Send-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                return new ArraySegment<byte>(retBuffer, 0, packageTotalLength);
            }, null);
        }

        /// <summary>
        /// 发送通知包
        /// </summary>
        public Task SendNoticePackage(object package)
        {
            var typeName = package.GetType().FullName;
            return SendNoticePackage(typeName, JsonSerializer.Serialize(package, GetNoticeTypeInfo(typeName)));
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
            var requestContent = JsonSerializer.Serialize(request, GetCommandRequestTypeInfo(typeName));
            await SendCommandRequestPackage(CommandContext.GenerateNewId(), typeName, requestContent).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送命令请求包
        /// </summary>
        public Task SendCommandRequestPackage(string commandId, string typeName, string content, Action afterSendHandler = null)
        {
            return writePackageAsync(buffer =>
            {
                //设置包类型
                buffer[PACKAGE_HEAD_LENGTH - 1] = (byte)QpPackageType.CommandRequest;
                //写入指令编号
                var commandIdBufferOffset = PACKAGE_HEAD_LENGTH;
                var commandIdBuffer = ByteUtils.HexDecode(commandId);
                Array.Copy(commandIdBuffer, 0, buffer, commandIdBufferOffset, commandIdBuffer.Length);

                var typeNameByteLengthOffset = commandIdBufferOffset + 16;
                //写入类名
                var typeNameByteOffset = typeNameByteLengthOffset + 1;
                var typeNameByteLength = encoding.GetEncoder().GetBytes(typeName.ToCharArray(), 0, typeName.Length, buffer, typeNameByteOffset, true);
                //写入类名长度
                buffer[typeNameByteLengthOffset] = Convert.ToByte(typeNameByteLength);

                var contentOffset = typeNameByteOffset + typeNameByteLength;
                var contentLength = encoding.GetByteCount(content);
                //如果内容超出了缓存可用空间的大小
                var retBuffer = buffer;
                if (contentLength > buffer.Length - contentOffset)
                {
                    retBuffer = new byte[contentOffset + contentLength];
                    Array.Copy(buffer, retBuffer, contentOffset);
                }
                encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, retBuffer, contentOffset, true);

                //包总长度
                var packageTotalLength = contentOffset + contentLength;

                if (LogUtils.LogCommand)
                    LogUtils.Log("{0}: [Send-CommandRequestPackage]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandId, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                return new ArraySegment<byte>(retBuffer, 0, packageTotalLength);
            }, afterSendHandler);
        }

        /// <summary>
        /// 发送命令响应包
        /// </summary>
        public Task SendCommandResponsePackage(string commandId, byte code, string message, string typeName, string content)
        {
            return writePackageAsync(buffer =>
            {
                //设置包类型
                buffer[PACKAGE_HEAD_LENGTH - 1] = (byte)QpPackageType.CommandResponse;
                //写入指令编号
                var commandIdBufferOffset = PACKAGE_HEAD_LENGTH;
                var commandIdBuffer = ByteUtils.HexDecode(commandId);
                Array.Copy(commandIdBuffer, 0, buffer, commandIdBufferOffset, commandIdBuffer.Length);
                //写入返回码
                var codeByteOffset = commandIdBufferOffset + commandIdBuffer.Length;
                buffer[codeByteOffset] = code;

                //如果是成功
                if (code == 0)
                {
                    var typeNameByteLengthOffset = codeByteOffset + 1;
                    //写入类名
                    var typeNameByteOffset = typeNameByteLengthOffset + 1;
                    var typeNameByteLength = encoding.GetEncoder().GetBytes(typeName.ToCharArray(), 0, typeName.Length, buffer, typeNameByteOffset, true);
                    //写入类名长度
                    buffer[typeNameByteLengthOffset] = Convert.ToByte(typeNameByteLength);

                    var contentOffset = typeNameByteOffset + typeNameByteLength;
                    var contentLength = encoding.GetByteCount(content);
                    //如果内容超出了缓存可用空间的大小
                    var retBuffer = buffer;
                    if (contentLength > buffer.Length - contentOffset)
                    {
                        retBuffer = new byte[contentOffset + contentLength];
                        Array.Copy(buffer, retBuffer, contentOffset);
                    }
                    encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, retBuffer, contentOffset, true);

                    //包总长度
                    var packageTotalLength = contentOffset + contentLength;

                    if (LogUtils.LogCommand)
                        LogUtils.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Type:{3},Content:{4}", DateTime.Now, commandId, code, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                    return new ArraySegment<byte>(retBuffer, 0, packageTotalLength);
                }
                //如果是失败
                else
                {
                    var messageOffset = codeByteOffset + 1;
                    var messageLength = encoding.GetByteCount(message);
                    //如果内容超出了缓存可用空间的大小
                    var retBuffer = buffer;
                    if (messageLength > buffer.Length - messageOffset)
                    {
                        retBuffer = new byte[messageOffset + messageLength];
                        Array.Copy(buffer, retBuffer, messageOffset);
                    }
                    encoding.GetEncoder().GetBytes(message.ToCharArray(), 0, message.Length, retBuffer, messageOffset, true);

                    //包总长度
                    var packageTotalLength = messageOffset + messageLength;

                    if (LogUtils.LogNotice)
                        LogUtils.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Message:{3}", DateTime.Now, commandId, code, message);

                    return new ArraySegment<byte>(retBuffer, 0, packageTotalLength);
                }
            }, null);
        }

        private async Task<int> readData(Stream stream, byte[] buffer, int startIndex, int totalCount, CancellationToken cancellationToken)
        {
            if (totalCount > buffer.Length - startIndex)
                throw new IOException($"Recv data length[{totalCount}] bigger than buffer length[{buffer.Length - startIndex}]");
            int ret;
            var count = 0;
            while (count < totalCount)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var readTask = stream.ReadAsync(buffer, count + startIndex, totalCount - count, cancellationToken);
                ret = await readTask
                    .WaitAsync(TimeSpan.FromMilliseconds(options.InternalTransportTimeout))
                    .ConfigureAwait(false);

                if (readTask.IsCanceled || ret == 0)
                    break;
                if (ret < 0)
                    throw new IOException("Read error from stream.");
                count += ret;
                if (options.EnableNetstat)
                {
                    BytesReceived += ret;
                    if (BytesReceived > LONG_HALF_MAX_VALUE)
                        BytesReceived = 0;
                }
            }
            return count;
        }

        /// <summary>
        /// 读取一个数据包
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<ArraySegment<byte>> ReadPackageAsync(CancellationToken token)
        {
            var stream = QpPackageHandler_Stream;
            if (stream == null)
                throw new ArgumentNullException(nameof(QpPackageHandler_Stream));

            //最终包缓存
            ArraySegment<byte> finalPackageBuffer;

            //是否正在读取拆分包
            bool isReadingSplitPackage = false;
            int splitMsCapacity = 0;
            MemoryStream splitMs = null;
            while (true)
            {
                if (token.IsCancellationRequested)
                    return nullArraySegment;

                var currentRecvBuffer = recvBuffer;
                //读取包头
                var ret = await readData(stream, currentRecvBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH, token).ConfigureAwait(false);
                if (ret == 0)
                    throw new IOException("未读取到数据！");
                if (ret < PACKAGE_TOTAL_LENGTH_LENGTH)
                    throw new ProtocolException(new ArraySegment<byte>(recvBuffer, 0, ret), $"包头读取错误！包头长度：{PACKAGE_TOTAL_LENGTH_LENGTH}，读取数据长度：{ret}");

                var currentRecvBuffer2 = recvBuffer;

                //如果读取缓存数组对象变化
                if (currentRecvBuffer != currentRecvBuffer2)
                    Array.Copy(currentRecvBuffer, currentRecvBuffer2, PACKAGE_TOTAL_LENGTH_LENGTH);

                //包总长度
                var packageTotalLength = ByteUtils.B2I_BE(recvBuffer, 0);
                if (packageTotalLength < PACKAGE_HEAD_LENGTH)
                    throw new ProtocolException(new ArraySegment<byte>(recvBuffer, 0, ret), $"包长度[{packageTotalLength}]必须大于等于{PACKAGE_HEAD_LENGTH}！");
                if (packageTotalLength > recvBuffer.Length)
                    throw new ProtocolException(new ArraySegment<byte>(recvBuffer, 0, ret), $"数据包总长度[{packageTotalLength}]大于缓存大小[{recvBuffer.Length}]");
                //包体长度
                var packageBodyLength = packageTotalLength - PACKAGE_TOTAL_LENGTH_LENGTH;

                if (token.IsCancellationRequested)
                    return nullArraySegment;
                //读取包体
                ret = await readData(stream, recvBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, packageBodyLength, token).ConfigureAwait(false);

                if (ret < packageBodyLength)
                    throw new ProtocolException(new ArraySegment<byte>(recvBuffer, 0, PACKAGE_HEAD_LENGTH + ret), $"包体读取错误！包体长度：{packageBodyLength}，读取数据长度：{ret}");

                var currentPackageBuffer = new ArraySegment<byte>(recvBuffer, 0, packageTotalLength);

                //如果设置了压缩或者加密
                if (options.InternalCompress || options.InternalEncrypt)
                {
                    //如果设置了加密，则先解密
                    if (options.InternalEncrypt)
                    {
                        var retBuffer = dec.TransformFinalBlock(currentPackageBuffer.Array, PACKAGE_TOTAL_LENGTH_LENGTH + currentPackageBuffer.Offset, currentPackageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH);
                        var currentBuffer = getFreeBuffer(currentPackageBuffer.Array, recvBuffer, recvBuffer2);
                        packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + retBuffer.Length;
                        writePackageTotalLengthToBuffer(currentBuffer, 0, packageTotalLength);
                        Array.Copy(retBuffer, 0, currentBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, retBuffer.Length);
                        currentPackageBuffer = new ArraySegment<byte>(currentBuffer, 0, packageTotalLength);
                    }
                    //如果设置了压缩，则先解压
                    if (options.InternalCompress)
                    {
                        var retBuffer = getFreeBuffer(currentPackageBuffer.Array, recvBuffer, recvBuffer2);
                        var count = 0;
                        using (var readMs = new MemoryStream(currentPackageBuffer.Array, PACKAGE_TOTAL_LENGTH_LENGTH + currentPackageBuffer.Offset, currentPackageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH, false))
                        using (var writeMs = new MemoryStream(retBuffer, 0, retBuffer.Length))
                        {
                            using (var gzStream = new GZipStream(readMs, CompressionMode.Decompress, true))
                                gzStream.CopyTo(writeMs);
                            count = Convert.ToInt32(writeMs.Position);
                        }
                        var currentBuffer = getFreeBuffer(retBuffer, recvBuffer, recvBuffer2);
                        packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + count;
                        writePackageTotalLengthToBuffer(currentBuffer, 0, packageTotalLength);
                        Array.Copy(retBuffer, 0, currentBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, count);
                        currentPackageBuffer = new ArraySegment<byte>(currentBuffer, 0, packageTotalLength);
                    }
                }
                var packageType = (QpPackageType)currentPackageBuffer.Array[currentPackageBuffer.Offset + PACKAGE_HEAD_LENGTH - 1];
                //如果当前包是拆分包
                if (packageType == QpPackageType.Split)
                {
                    if (!isReadingSplitPackage)
                    {
                        var tmpPackageBodyLength = ByteUtils.B2I_BE(currentPackageBuffer.Array, currentPackageBuffer.Offset + PACKAGE_HEAD_LENGTH);
                        splitMsCapacity = tmpPackageBodyLength;
                        if (splitMsCapacity <= 0)
                            throw new IOException($"拆分包中包长度[{splitMsCapacity}]必须为正数！");
                        if (splitMsCapacity > options.MaxPackageSize)
                            throw new IOException($"拆分包中包长度[{splitMsCapacity}]大于最大包大小[{options.MaxPackageSize}]");
                        splitMs = new MemoryStream(splitMsCapacity);
                        isReadingSplitPackage = true;
                    }
                    await splitMs.WriteAsync(currentPackageBuffer.Array, currentPackageBuffer.Offset + PACKAGE_HEAD_LENGTH, currentPackageBuffer.Count - PACKAGE_HEAD_LENGTH).ConfigureAwait(false);

                    //如果拆分包已经读取完成
                    if (splitMs.Position >= splitMsCapacity)
                    {
                        finalPackageBuffer = new ArraySegment<byte>(splitMs.ToArray());
                        splitMs.Dispose();
                        if (LogUtils.LogSplit)
                            LogUtils.Log("{0}: [Recv-SplitPackage]Length:{1}", DateTime.Now, finalPackageBuffer.Count);
                        break;
                    }
                }
                else
                {
                    finalPackageBuffer = currentPackageBuffer;
                    break;
                }
            }

            if (LogUtils.LogPackage)
                LogUtils.Log(
                "{0}: [Recv-Package]Length:{1}，Type:{2}，Content:{3}",
                DateTime.Now,
                finalPackageBuffer.Count,
                (QpPackageType)finalPackageBuffer.Array[finalPackageBuffer.Offset + PACKAGE_HEAD_LENGTH - 1],
                LogUtils.LogContent ?
                    BitConverter.ToString(finalPackageBuffer.Array, finalPackageBuffer.Offset, finalPackageBuffer.Count)
                    : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

            return new ArraySegment<byte>(finalPackageBuffer.Array,
                    finalPackageBuffer.Offset,
                    finalPackageBuffer.Count);
        }

        protected void BeginHeartBeat(CancellationToken cancellationToken)
        {
            if (options.HeartBeatInterval > 0)
                Task.Delay(options.HeartBeatInterval, cancellationToken).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        return;
                    if (QpPackageHandler_Stream == null)
                        return;

                    var lastSendPackageToNowSeconds = (DateTime.Now - lastSendPackageTime).TotalMilliseconds;

                    //如果离最后一次发送数据包的时间大于心跳间隔，则发送心跳包
                    if (lastSendPackageToNowSeconds > options.HeartBeatInterval)
                    {
                        SendHeartbeatPackage();
                    }
                    BeginHeartBeat(cancellationToken);
                });
        }

        /// <summary>
        /// 接收到原始通知数据包时
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        protected void OnRawNoticePackageReceived(string typeName, string content)
        {
            //触发RawNoticePackageReceived事件
            RawNoticePackageReceived?.Invoke(this, new RawNoticePackageReceivedEventArgs()
            {
                TypeName = typeName,
                Content = content
            });
            //如果在字典中未找到此类型名称，则直接返回
            if (!noticeTypeDict.ContainsKey(typeName))
                return;
            var contentModel = JsonSerializer.Deserialize(content, noticeTypeDict[typeName]);

            //处理通知
            var hasNoticeHandler = false;
            if (options.NoticeHandlerManagerList != null)
                foreach (var noticeHandlerManager in options.NoticeHandlerManagerList)
                {
                    if (noticeHandlerManager.CanHandleNoticed(typeName))
                    {
                        hasNoticeHandler = true;
                        noticeHandlerManager.HandleNotice(this, typeName, contentModel);
                        break;
                    }
                }

            //如果配置了触发NoticePackageReceived事件
            if (options.RaiseNoticePackageReceivedEvent)
            {
                NoticePackageReceived?.Invoke(this, new NoticePackageReceivedEventArgs()
                {
                    TypeName = typeName,
                    ContentModel = contentModel,
                    Handled = hasNoticeHandler
                });
            }
        }

        /// <summary>
        /// 接收到命令请求数据包时
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        private void OnCommandRequestReceived(string commandId, string typeName, string content)
        {
            var eventArgs = new RawCommandRequestPackageReceivedEventArgs()
            {
                CommandId = commandId,
                TypeName = typeName,
                Content = content
            };
            RawCommandRequestPackageReceived?.Invoke(this, eventArgs);
            //如果已经处理，则直接返回
            if (eventArgs.Handled)
                return;

            try
            {
                //如果在字典中未找到此类型名称，则直接返回
                if (!commandRequestTypeDict.ContainsKey(typeName))
                    throw new CommandException(255, $"Unknown RequestType: {typeName}.");

                var cmdRequestType = commandRequestTypeDict[typeName];
                var cmdResponseType = commandRequestTypeResponseTypeDict[cmdRequestType];

                var contentModel = JsonSerializer.Deserialize(content, cmdRequestType);
                CommandRequestPackageReceived?.Invoke(this, new CommandRequestPackageReceivedEventArgs()
                {
                    CommandId = commandId,
                    TypeName = typeName,
                    ContentModel = contentModel
                });

                var hasCommandExecuter = false;
                if (options.CommandExecuterManagerList != null)
                    foreach (var commandExecuterManager in options.CommandExecuterManagerList)
                    {
                        if (commandExecuterManager.CanExecuteCommand(typeName))
                        {
                            hasCommandExecuter = true;
                            var responseModel = commandExecuterManager.ExecuteCommand(this, typeName, contentModel);
                            SendCommandResponsePackage(commandId, 0, null, cmdResponseType.Type.FullName, JsonSerializer.Serialize(responseModel, cmdResponseType));
                            break;
                        }
                    }
                if (!hasCommandExecuter)
                    throw new CommandException(255, $"No CommandExecuter for RequestType:{typeName}");
            }
            catch (CommandException ex)
            {
                string errorMessage = ExceptionUtils.GetExceptionMessage(ex);
                SendCommandResponsePackage(commandId, ex.Code, errorMessage, null, null);
            }
            catch (Exception ex)
            {
                string errorMessage = ExceptionUtils.GetExceptionMessage(ex);
                SendCommandResponsePackage(commandId, 255, errorMessage, null, null);
            }
        }

        /// <summary>
        /// 接收到命令响应数据包时
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="typeName"></param>
        /// <param name="content"></param>
        private void OnCommandResponseReceived(string commandId, byte code, string message, string typeName, string content)
        {
            CommandResponsePackageReceived?.Invoke(this, new CommandResponsePackageReceivedEventArgs()
            {
                CommandId = commandId,
                Code = code,
                Message = message,
                TypeName = typeName,
                Content = content
            });
            //设置指令响应
            CommandContext commandContext;
            if (!commandDict.TryRemove(commandId, out commandContext))
                return;
            if (code == 0)
                commandContext.SetResponse(typeName, content);
            else
                commandContext.SetResponse(new CommandException(code, message));
        }


        protected void BeginNetstat(CancellationToken cancellationToken)
        {
            if (!options.EnableNetstat)
                return;
            if (cancellationToken.IsCancellationRequested)
                return;

            long preBytesReceived = BytesReceived;
            long preBytesSent = BytesSent;

            Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ContinueWith(t =>
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

        protected void BeginReadPackage(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var readPackageTask = ReadPackageAsync(token);
            readPackageTask.ContinueWith(t =>
            {
                //如果已经取消
                if (t.IsCanceled || token.IsCancellationRequested)
                    return;

                //如果读取出错
                if (t.IsFaulted)
                {
                    OnReadError(t.Exception.InnerException);
                    return;
                }
                //解析包
                try
                {
                    var package = t.Result;
                    if (package.Count > 0)
                    {
                        var packageType = (QpPackageType)package.Array[package.Offset + PACKAGE_HEAD_LENGTH - 1];
                        switch (packageType)
                        {
                            case QpPackageType.Heartbeat:
                                {
                                    if (LogUtils.LogHeartbeat)
                                        LogUtils.Log("{0}: [Recv-HeartbeatPackage]", DateTime.Now);
                                    HeartbeatPackageReceived?.Invoke(this, QpEventArgs.Empty);
                                    break;
                                }
                            case QpPackageType.Notice:
                                {
                                    var typeNameLengthOffset = package.Offset + PACKAGE_HEAD_LENGTH;
                                    var typeNameLength = package.Array[typeNameLengthOffset];

                                    var typeNameOffset = typeNameLengthOffset + 1;
                                    var typeName = encoding.GetString(package.Array, typeNameOffset, typeNameLength);

                                    var contentOffset = typeNameOffset + typeNameLength;
                                    var content = encoding.GetString(package.Array, contentOffset, package.Offset + package.Count - contentOffset);

                                    if (LogUtils.LogNotice)
                                        LogUtils.Log("{0}: [Recv-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                                    OnRawNoticePackageReceived(typeName, content);
                                    break;
                                }
                            case QpPackageType.CommandRequest:
                                {
                                    var commandIdOffset = package.Offset + PACKAGE_HEAD_LENGTH;
                                    var commandId = BitConverter.ToString(package.Array, commandIdOffset, COMMAND_ID_LENGTH).Replace("-", string.Empty).ToLower();

                                    var typeNameLengthOffset = commandIdOffset + COMMAND_ID_LENGTH;
                                    var typeNameLength = package.Array[typeNameLengthOffset];

                                    var typeNameOffset = typeNameLengthOffset + 1;
                                    var typeName = encoding.GetString(package.Array, typeNameOffset, typeNameLength);

                                    var contentOffset = typeNameOffset + typeNameLength;
                                    var content = encoding.GetString(package.Array, contentOffset, package.Offset + package.Count - contentOffset);

                                    if (LogUtils.LogCommand)
                                        LogUtils.Log("{0}: [Recv-CommandRequestPackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
                                    //异步执行命令请求事件处理器
                                    Task.Run(() => OnCommandRequestReceived(commandId, typeName, content));
                                    break;
                                }
                            case QpPackageType.CommandResponse:
                                {
                                    var commandIdOffset = package.Offset + PACKAGE_HEAD_LENGTH;
                                    var commandId = BitConverter.ToString(package.Array, commandIdOffset, COMMAND_ID_LENGTH).Replace("-", string.Empty).ToLower();

                                    var codeOffset = commandIdOffset + COMMAND_ID_LENGTH;
                                    var code = package.Array[codeOffset];

                                    string typeName = null;
                                    string content = null;
                                    string message = null;

                                    //如果成功
                                    if (code == 0)
                                    {
                                        var typeNameLengthOffset = codeOffset + 1;
                                        var typeNameLength = package.Array[typeNameLengthOffset];

                                        var typeNameOffset = typeNameLengthOffset + 1;
                                        typeName = encoding.GetString(package.Array, typeNameOffset, typeNameLength);

                                        var contentOffset = typeNameOffset + typeNameLength;
                                        content = encoding.GetString(package.Array, contentOffset, package.Offset + package.Count - contentOffset);
                                    }
                                    else
                                    {
                                        var messageOffset = codeOffset + 1;
                                        message = encoding.GetString(package.Array, messageOffset, package.Offset + package.Count - messageOffset);
                                    }

                                    if (LogUtils.LogCommand)
                                        LogUtils.Log("{0}: [Recv-CommandResponsePackage]Code:{1}，Message：{2}，Type:{3},Content:{4}", DateTime.Now, code, message, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                                    OnCommandResponseReceived(commandId, code, message, typeName, content);
                                    break;
                                }
                        }
                    }
                    //读取下一个数据包
                    BeginReadPackage(token);
                }
                catch (Exception ex)
                {
                    OnReadError(ex);
                }
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

        public async Task<TCmdResponse> SendCommand<TCmdRequest, TCmdResponse>(TCmdRequest request, int timeout = 30 * 1000, Action afterSendHandler = null)
            where TCmdRequest : IQpCommandRequest<TCmdResponse>
        {
            var typeName = typeof(TCmdRequest).FullName;
            var requestTypeInfo = GetCommandRequestTypeInfo(typeName);
            var requestContent = JsonSerializer.Serialize(request, requestTypeInfo);

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
            var responseTypeInfo = GetCommandResponseTypeInfo(ret.TypeName);
            return (TCmdResponse)JsonSerializer.Deserialize(ret.Content, responseTypeInfo);
        }
    }
}
