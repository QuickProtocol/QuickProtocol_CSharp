using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol
{
    public class QpServerChannel : QpChannel
    {
        private QpServer server;
        private Stream stream;
        private CancellationTokenSource cts;
        private QpServerOptions options;
        private string question;
        //通过认证后，才允许使用的命令执行管理器列表
        private List<CommandExecuterManager> authedCommandExecuterManagerList = null;

        public string ChannelName { get; private set; }

        /// <summary>
        /// 通过认证时
        /// </summary>
        public event EventHandler Auchenticated;

        /// <summary>
        /// 连接断开时
        /// </summary>
        public event EventHandler Disconnected;

        public QpServerChannel(QpServer server, Stream stream, string channelName, CancellationToken cancellationToken, QpServerOptions options) : base(options)
        {
            this.server = server;
            this.stream = stream;
            this.ChannelName = channelName;
            this.options = options;
            this.authedCommandExecuterManagerList = options.CommandExecuterManagerList;
            cts = new CancellationTokenSource();
            cancellationToken.Register(() => Stop());
            //修改缓存大小
            ChangeBufferSize(options.BufferSize);
            IsConnected = true;

            //初始化连接相关指令处理器
            var connectAndAuthCommandExecuterManager = new CommandExecuterManager();
            connectAndAuthCommandExecuterManager.Register(new Commands.Connect.Request(), connect);
            connectAndAuthCommandExecuterManager.Register(new Commands.Authenticate.Request(), authenticate);
            connectAndAuthCommandExecuterManager.Register(new Commands.HandShake.Request(), handShake);
            connectAndAuthCommandExecuterManager.Register(new Commands.GetQpInstructions.Request(), getQpInstructions);
            options.CommandExecuterManagerList = new List<CommandExecuterManager>() { connectAndAuthCommandExecuterManager };

            InitQpPackageHandler_Stream(stream);
            //开始读取其他数据包
            BeginReadPackage(cts.Token);
        }

        private Commands.Connect.Response connect(QpChannel handler, Commands.Connect.Request request)
        {
            if (request.InstructionIds != null)
            {
                foreach (var id in request.InstructionIds.Where(t => !string.IsNullOrEmpty(t)))
                {
                    if (!options.InstructionSet.Any(t => t.Id == id))
                        throw new CommandException(255, $"Unknown instruction: {id}");
                }
            }

            question = Guid.NewGuid().ToString("N");
            return new Commands.Connect.Response()
            {
                BufferSize = options.BufferSize,
                Question = question
            };
        }

        private Commands.Authenticate.Response authenticate(QpChannel handler, Commands.Authenticate.Request request)
        {
            if (Utils.CryptographyUtils.ComputeMD5Hash(question + options.Password) != request.Answer)
            {
                Task.Delay(1000).ContinueWith(t =>
                {
                    Stop();
                });
                throw new CommandException(1, "认证失败！");
            }
            Auchenticated?.Invoke(this, EventArgs.Empty);
            return new Commands.Authenticate.Response();
        }

        private Commands.HandShake.Response handShake(QpChannel handler, Commands.HandShake.Request request)
        {
            options.CommandExecuterManagerList.AddRange(authedCommandExecuterManagerList);
            options.InternalCompress = request.EnableCompress;
            options.InternalEncrypt = request.EnableEncrypt;
            options.InternalTransportTimeout = request.TransportTimeout;

            //改变传输超时时间
            ChangeTransportTimeout();

            //开始心跳
            if (options.HeartBeatInterval > 0)
                BeginHeartBeat(cts.Token);
            return new Commands.HandShake.Response();
        }

        private Commands.GetQpInstructions.Response getQpInstructions(QpChannel handler, Commands.GetQpInstructions.Request request)
        {
            return new Commands.GetQpInstructions.Response()
            {
                Data = options.InstructionSet
            };
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            try
            {
                if (cts != null && !cts.IsCancellationRequested)
                    cts.Cancel();
                stream?.Close();
                stream?.Dispose();
            }
            catch { }
        }

        protected override void OnReadError(Exception exception)
        {
            if (options.ProtocolErrorHandler != null)
            {
                if (exception is ProtocolException)
                {
                    var protocolException = (ProtocolException)exception;
                    server.RemoveChannel(this);
                    if (LogUtils.LogConnection)
                        LogUtils.Log("[ProtocolErrorHandler]{0}: Begin ProtocolErrorHandler invoke...", DateTime.Now);

                    options.ProtocolErrorHandler.Invoke(stream, protocolException.ReadBuffer);
                    return;
                }
            }
            Stop();
            base.OnReadError(exception);
            if (IsConnected)
            {
                IsConnected = false;
                Disconnected?.Invoke(this, QpEventArgs.Empty);
            }
        }
    }
}
