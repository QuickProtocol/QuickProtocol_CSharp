using Newtonsoft.Json;
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

        private DES des = DES.Create();
        private byte[] passwordMd5Buffer;
        private ICryptoTransform enc;
        private ICryptoTransform dec;
        private Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// 当时是否连接
        /// </summary>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// 最后的异常
        /// </summary>
        public Exception LastException { get; private set; }

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
        public Object Tag { get; set; }

        private Dictionary<string, Type> noticeTypeDict = new Dictionary<string, Type>();

        public QpChannel(QpChannelOptions options)
        {
            this.options = options;
            ChangeBufferSize(BufferSize);
            passwordMd5Buffer = CryptographyUtils.ComputeMD5Hash(Encoding.UTF8.GetBytes(options.Password)).Take(8).ToArray();
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
                    }
                }
            }

            foreach (var instructionSet in options.InstructionSet)
            {
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
                    }
                }
            }
        }

        protected void InitQpPackageHandler_Stream(Stream stream)
        {
            if (QpPackageHandler_Stream != null)
                try { QpPackageHandler_Stream?.Dispose(); }
                catch { }
            QpPackageHandler_Stream = stream;

            options.InternalCompress = false;
            options.InternalEncrypt = false;
            ChangeTransportTimeout();            
        }

        /// <summary>
        /// 当读取出错时
        /// </summary>
        protected virtual void OnReadError(Exception exception)
        {
            LastException = exception;
            LogUtils.Log("[ReadError]{0}: {1}", DateTime.Now, ExceptionUtils.GetExceptionString(exception));
            InitQpPackageHandler_Stream(null);
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

        private void sendPackage(Func<byte[], Tuple<int, byte[]>> getPackagePayloadFunc, Action afterSendHandler = null)
        {
            var stream = QpPackageHandler_Stream;
            if (stream == null)
                return;

            lock (this)
            {
                var ret = getPackagePayloadFunc(sendBuffer);
                var packageTotalLength = ret.Item1;
                if (packageTotalLength < PACKAGE_HEAD_LENGTH)
                    throw new IOException($"包大小[{packageTotalLength}]小于包头长度[{PACKAGE_HEAD_LENGTH}]");

                byte[] packageBuffer = ret.Item2;

                //构造包头
                writePackageTotalLengthToBuffer(packageBuffer, 0, packageTotalLength);
                try
                {
                    //如果包缓存是发送缓存
                    if (packageBuffer == sendBuffer)
                    {
                        sendPackageBuffer(stream,
                            new ArraySegment<byte>(packageBuffer, 0, packageTotalLength),
                            afterSendHandler
                            );
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
                            sendPackageBuffer(
                                stream,
                                new ArraySegment<byte>(sendBuffer, 0, PACKAGE_HEAD_LENGTH + takeLength),
                                afterSendHandler);
                            currentIndex += takeLength;
                        }
                    }
                    lastSendPackageTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LogUtils.Log("[SendPackage]" + ExceptionUtils.GetExceptionString(ex));
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        public void SendHeartbeatPackage()
        {
            sendPackage(buffer =>
            {
                HEARTBEAT_PACKAGHE.CopyTo(buffer, 0);
                return new Tuple<int, byte[]>(HEARTBEAT_PACKAGHE.Length, buffer);
            });
        }

        public void SendNoticePackage(string noticePackageTypeName, string noticePackageContent)
        {
            sendPackage(buffer =>
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
                    encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, retBuffer, contentOffset, true);
                }
                else
                {
                    contentLength = encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, buffer, contentOffset, true);
                }
                //包总长度
                var packageTotalLength = contentOffset + contentLength;

                if (LogUtils.LogNotice)
                    LogUtils.Log("{0}: [Send-NoticePackage]Type:{1},Content:{2}", DateTime.Now, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                return new Tuple<int, byte[]>(packageTotalLength, retBuffer);
            });
        }

        /// <summary>
        /// 发送通知包
        /// </summary>
        public void SendNoticePackage(object package)
        {
            SendNoticePackage(package.GetType().FullName, JsonConvert.SerializeObject(package));
        }

        /// <summary>
        /// 发送命令请求包
        /// </summary>
        public void SendCommandRequestPackage(string commandId, string typeName, string content, Action afterSendHandler = null)
        {
            sendPackage(buffer =>
            {
                //设置包类型
                buffer[PACKAGE_HEAD_LENGTH - 1] = (byte)QpPackageType.CommandRequest;
                //写入指令编号
                var commandIdBufferOffset = PACKAGE_HEAD_LENGTH;
                var commandIdBuffer = Guid.Parse(commandId).ToByteArray();
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
                    encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, retBuffer, contentOffset, true);
                }
                else
                {
                    contentLength = encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, buffer, contentOffset, true);
                }
                //包总长度
                var packageTotalLength = contentOffset + contentLength;

                if (LogUtils.LogCommand)
                    LogUtils.Log("{0}: [Send-CommandRequestPackage]CommandId:{1},Type:{2},Content:{3}", DateTime.Now, commandId, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                return new Tuple<int, byte[]>(packageTotalLength, retBuffer);
            }, afterSendHandler);
        }

        /// <summary>
        /// 发送命令响应包
        /// </summary>
        public void SendCommandResponsePackage(string commandId, byte code, string message, string typeName, string content)
        {
            sendPackage(buffer =>
            {
                //设置包类型
                buffer[PACKAGE_HEAD_LENGTH - 1] = (byte)QpPackageType.CommandResponse;
                //写入指令编号
                var commandIdBufferOffset = PACKAGE_HEAD_LENGTH;
                var commandIdBuffer = Guid.Parse(commandId).ToByteArray();
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
                        encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, retBuffer, contentOffset, true);
                    }
                    else
                    {
                        contentLength = encoding.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, buffer, contentOffset, true);
                    }
                    //包总长度
                    var packageTotalLength = contentOffset + contentLength;

                    if (LogUtils.LogCommand)
                        LogUtils.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Type:{3},Content:{4}", DateTime.Now, commandId, code, typeName, LogUtils.LogContent ? content : LogUtils.NOT_SHOW_CONTENT_MESSAGE);

                    return new Tuple<int, byte[]>(packageTotalLength, retBuffer);
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
                        encoding.GetEncoder().GetBytes(message.ToCharArray(), 0, message.Length, retBuffer, messageOffset, true);
                    }
                    else
                    {
                        messageLength = encoding.GetEncoder().GetBytes(message.ToCharArray(), 0, message.Length, buffer, messageOffset, true);
                    }
                    //包总长度
                    var packageTotalLength = messageOffset + messageLength;

                    if (LogUtils.LogNotice)
                        LogUtils.Log("{0}: [Send-CommandResponsePackage]CommandId:{1},Code:{2},Message:{3}", DateTime.Now, commandId, code, message);

                    return new Tuple<int, byte[]>(packageTotalLength, retBuffer);
                }
            });
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

        private void sendPackageBuffer(Stream stream, ArraySegment<byte> packageBuffer, Action afterSendHandler = null)
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
                            gzStream.Write(packageBuffer.Array, packageBuffer.Offset + PACKAGE_TOTAL_LENGTH_LENGTH, packageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH);
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
            stream.Write(packageBuffer.Array, packageBuffer.Offset, packageBuffer.Count);
            if (LogUtils.LogPackage)
                LogUtils.Log(
                    "{0}: [Send-Package]Length:{1}，Type:{2}，Content:{3}",
                    DateTime.Now,
                    packageBuffer.Count,
                    (QpPackageType)packageType,
                    LogUtils.LogContent ?
                        BitConverter.ToString(packageBuffer.Array, packageBuffer.Offset, packageBuffer.Count)
                        : LogUtils.NOT_SHOW_CONTENT_MESSAGE);
            stream.Flush();
        }

        private async Task<int> readData(Stream stream, byte[] buffer, int startIndex, int totalCount, CancellationToken cancellationToken)
        {
            if (totalCount > buffer.Length - startIndex)
                throw new IOException($"要接收的数据大小[{totalCount}]超出了缓存的大小[{buffer.Length - startIndex}]！");
            int ret;
            var count = 0;
            while (count < totalCount)
            {
                var readTask = stream.ReadAsync(buffer, count + startIndex, totalCount - count, cancellationToken);
                ret = await await TaskUtils.TaskWait(readTask, options.InternalTransportTimeout);
                if (readTask.IsCanceled || ret == 0)
                    break;
                if (ret < 0)
                    throw new IOException("从网络流中读取错误！");
                count += ret;
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
                var currentRecvBuffer = recvBuffer;
                //读取包头
                var ret = await readData(stream, currentRecvBuffer, 0, PACKAGE_TOTAL_LENGTH_LENGTH, token);
                if (token.IsCancellationRequested)
                    return nullArraySegment;
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
                //读取包体
                ret = await readData(stream, recvBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, packageBodyLength, token);

                if (token.IsCancellationRequested)
                    return nullArraySegment;
                if (ret < packageBodyLength)
                    throw new ProtocolException(new ArraySegment<byte>(recvBuffer, 0, PACKAGE_HEAD_LENGTH + ret), $"包体读取错误！包体长度：{packageBodyLength}，读取数据长度：{ret}");

                var currentPackageBuffer = new ArraySegment<byte>(recvBuffer, 0, packageTotalLength);

                //如果设置了压缩或者加密
                if (options.InternalCompress || options.InternalEncrypt)
                {
                    //如果设置了加密，则先解密
                    if (options.InternalEncrypt)
                    {
                        try
                        {
                            var retBuffer = dec.TransformFinalBlock(currentPackageBuffer.Array, PACKAGE_TOTAL_LENGTH_LENGTH + currentPackageBuffer.Offset, currentPackageBuffer.Count - PACKAGE_TOTAL_LENGTH_LENGTH);
                            var currentBuffer = getFreeBuffer(currentPackageBuffer.Array, recvBuffer, recvBuffer2);
                            packageTotalLength = PACKAGE_TOTAL_LENGTH_LENGTH + retBuffer.Length;
                            writePackageTotalLengthToBuffer(currentBuffer, 0, packageTotalLength);
                            Array.Copy(retBuffer, 0, currentBuffer, PACKAGE_TOTAL_LENGTH_LENGTH, retBuffer.Length);
                            currentPackageBuffer = new ArraySegment<byte>(currentBuffer, 0, packageTotalLength);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
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
                    splitMs.Write(currentPackageBuffer.Array, currentPackageBuffer.Offset + PACKAGE_HEAD_LENGTH, currentPackageBuffer.Count - PACKAGE_HEAD_LENGTH);

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
            if (QpPackageHandler_Stream == null)
                return;

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

        protected void BeginReadPackage(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var readPackageTask = ReadPackageAsync(token);
            readPackageTask.ContinueWith(t =>
            {
                if (QpPackageHandler_Stream == null)
                    return;
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
                var package = t.Result;
                if (package.Count > 0)
                {
                    var packageType = (QpPackageType)package.Array[package.Offset + PACKAGE_HEAD_LENGTH - 1];
                    switch (packageType)
                    {
                        case QpPackageType.Heartbeat:
                            {
                                if (LogUtils.LogHeartbeat)
                                    LogUtils.Log("{0}: [Recv-HeartbetaPackage]", DateTime.Now);
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

                                OnCommandRequestReceived(commandId, typeName, content);
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

            //如果配置了触发NoticePackageReceived事件
            if (options.RaiseNoticePackageReceivedEvent)
            {
                //如果在字典中未找到此类型名称，则直接返回
                if (!noticeTypeDict.ContainsKey(typeName))
                    return;
                var contentModel = JsonConvert.DeserializeObject(content, noticeTypeDict[typeName]);
                NoticePackageReceived?.Invoke(this, new NoticePackageReceivedEventArgs()
                {
                    TypeName = typeName,
                    ContentModel = contentModel
                });
            }
        }

        private Dictionary<string, Type> commandRequestTypeDict = new Dictionary<string, Type>();
        private Dictionary<string, Type> commandResponseTypeDict = new Dictionary<string, Type>();
        private Dictionary<Type, Type> commandRequestTypeResponseTypeDict = new Dictionary<Type, Type>();

        private ConcurrentDictionary<string, CommandContext> commandDict = new ConcurrentDictionary<string, CommandContext>();

        /// <summary>
        /// 原始收到命令请求数据包事件
        /// </summary>
        public event EventHandler<RawCommandRequestPackageReceivedEventArgs> RawCommandRequestPackageReceived;
        /// <summary>
        /// 收到命令请求数据包事件
        /// </summary>
        public event EventHandler<CommandRequestPackageReceivedEventArgs> CommandRequestPackageReceived;
        /// <summary>
        /// 原始收到命令响应数据包事件
        /// </summary>
        public event EventHandler<RawCommandResponsePackageReceivedEventArgs> RawCommandResponsePackageReceived;
        /// <summary>
        /// 收到命令响应数据包事件
        /// </summary>
        public event EventHandler<CommandResponsePackageReceivedEventArgs> CommandResponsePackageReceived;

        /// <summary>
        /// 添加命令执行器管理器
        /// </summary>
        /// <param name="commandExecuterManager"></param>
        public void AddCommandExecuterManager(CommandExecuterManager commandExecuterManager)
        {
            options.CommandExecuterManagerList.Add(commandExecuterManager);
        }


        public async Task<CommandResponseTypeNameAndContent> SendCommand(string requestTypeName, string requestContent, int timeout = 30 * 1000, Action afterSendHandler = null)
        {
            var commandContext = new CommandContext(requestTypeName);
            commandDict.TryAdd(commandContext.Id, commandContext);

            if (timeout <= 0)
            {
                SendCommandRequestPackage(commandContext.Id, requestTypeName, requestContent, afterSendHandler);
                return await commandContext.ResponseTask;
            }
            //如果设置了超时
            else
            {
                try
                {
                    await TaskUtils.TaskWait(Task.Run(() => SendCommandRequestPackage(commandContext.Id, requestTypeName, requestContent, afterSendHandler)), timeout);
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
                return await await TaskUtils.TaskWait(commandContext.ResponseTask, timeout);
            }
        }

        public async Task<TCmdResponse> SendCommand<TCmdResponse>(IQpCommandRequest<TCmdResponse> request, int timeout = 30 * 1000, Action afterSendHandler = null)
        {
            var requestType = request.GetType();
            var typeName = requestType.FullName;
            var requestContent = JsonConvert.SerializeObject(request);

            var commandContext = new CommandContext(typeName);
            commandDict.TryAdd(commandContext.Id, commandContext);

            CommandResponseTypeNameAndContent ret = null;
            if (timeout <= 0)
            {
                SendCommandRequestPackage(commandContext.Id, typeName, requestContent, afterSendHandler);
                ret = await commandContext.ResponseTask;
            }
            //如果设置了超时
            else
            {
                try
                {
                    await TaskUtils.TaskWait(Task.Run(() => SendCommandRequestPackage(commandContext.Id, typeName, requestContent, afterSendHandler)), timeout);
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
                ret = await await TaskUtils.TaskWait(commandContext.ResponseTask, timeout);
            }
            return JsonConvert.DeserializeObject<TCmdResponse>(ret.Content);
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

                var contentModel = JsonConvert.DeserializeObject(content, cmdRequestType);
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
                            SendCommandResponsePackage(commandId, 0, null, cmdResponseType.FullName, JsonConvert.SerializeObject(responseModel));
                            break;
                        }
                    }
                if (!hasCommandExecuter)
                    throw new CommandException(255, $"No CommandExecuter for RequestType:{typeName}");
            }
            catch (CommandException ex)
            {
                string errorMessage = ExceptionUtils.GetExceptionString(ex);
                SendCommandResponsePackage(commandId, ex.Code, errorMessage, null, null);
            }
            catch (Exception ex)
            {
                string errorMessage = ExceptionUtils.GetExceptionString(ex);
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
            RawCommandResponsePackageReceived?.Invoke(this, new RawCommandResponsePackageReceivedEventArgs()
            {
                CommandId = commandId,
                Code = code,
                Message = message,
                TypeName = typeName,
                Content = content
            });

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
    }
}
