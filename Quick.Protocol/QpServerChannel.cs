using Quick.Protocol.Exceptions;

namespace Quick.Protocol;

public class QpServerChannel : QpChannel
{
    private  Stream stream;
    private readonly CancellationTokenSource cts;
    private readonly CancellationToken serverCancellationToken;
    public new QpServerOptions Options { get; }
    private readonly string channelName;
    //通过认证后，才允许使用的命令执行管理器列表
    private readonly List<CommandExecuterManager> authedCommandExecuterManagerList = null;
    //通过认证后，才允许使用的通知处理器管理器列表
    private readonly List<NoticeHandlerManager> authedNoticeHandlerManagerList = null;

    public override string ChannelName => channelName;
    public Stream GetStream() => stream;

    /// <summary>
    /// 通过认证时
    /// </summary>
    internal event EventHandler Auchenticated;

    protected override bool ReadFromStreamReturnZeroMeansFault { get; }

    public QpServerChannel(Stream stream, string channelName, CancellationToken cancellationToken, QpServerOptions options, bool readFromStreamReturnZeroMeansFault = true) : base(options)
    {
        this.stream = stream;
        this.channelName = channelName;
        Options = options;
        authedCommandExecuterManagerList = options.CommandExecuterManagerList;
        authedNoticeHandlerManagerList = options.NoticeHandlerManagerList;
        serverCancellationToken = cancellationToken;
        ReadFromStreamReturnZeroMeansFault = readFromStreamReturnZeroMeansFault;

        IsConnected = true;

        //初始化连接相关指令处理器
        var connectAndAuthCommandExecuterManager = new CommandExecuterManager();
        connectAndAuthCommandExecuterManager.Register(new Commands.Connect.Request(), connect);
        connectAndAuthCommandExecuterManager.Register(new Commands.Authenticate.Request(), authenticate);
        connectAndAuthCommandExecuterManager.Register(new Commands.HandShake.Request(), handShake);
        connectAndAuthCommandExecuterManager.Register(new Commands.GetQpInstructions.Request(), getQpInstructions);
        options.CommandExecuterManagerList = new List<CommandExecuterManager>() { connectAndAuthCommandExecuterManager };
        options.NoticeHandlerManagerList = null;

        InitQpPackageHandler_Stream(stream);
        cts = new CancellationTokenSource();
    }

    internal void Start()
    {
        var token = cts.Token;
        //开始读取其他数据包
        BeginReadPackage(token);
        //开始统计网络数据
        _ = BeginNetstat(token);
        //开始检查服务端的取消令牌
        _ = BeginCheckServerCancellationToken(token);

        //如果认证超时时间后没有通过认证，则断开连接
        if (Options.AuthenticateTimeout > 0)
            _ = Task.Delay(Options.AuthenticateTimeout, token).ContinueWith(t =>
            {
                //如果已经取消或者已经通过认证
                if (t.IsCanceled
                || IsAuchenticated)
                    return;
                if (Options.Logger is { LogConnection: true })
                    Options.Logger.Log("{0} Authenticate timeout.", channelName);
                Stop();
                LastException = new TimeoutException("Authenticate timeout");
                Disconnect();
            });
    }

    private async Task BeginCheckServerCancellationToken(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
            if (serverCancellationToken.IsCancellationRequested)
                Stop();
        }
    }

    private Commands.Connect.Response connect(QpChannel handler, Commands.Connect.Request request)
    {
        if (request.InstructionIds != null)
        {
            foreach (var id in request.InstructionIds.Where(t => !string.IsNullOrEmpty(t)))
            {
                if (!Options.InstructionSet.Any(t => t.Id == id))
                    throw new CommandException(255, $"Unknown instruction: {id}");
            }
        }
        AuthenticateQuestion = Guid.NewGuid().ToString("N");
        return new Commands.Connect.Response()
        {
            Question = AuthenticateQuestion
        };
    }

    private Commands.Authenticate.Response authenticate(QpChannel handler, Commands.Authenticate.Request request)
    {
        try
        {
            if (Utils.CryptographyUtils.ComputeMD5Hash(AuthenticateQuestion + Options.Password) != request.Answer)
            {
                _ = Task.Delay(1000).ContinueWith(t =>
                {
                    Stop();
                });
                throw new CommandException(1, "Authenticate failed.");
            }
            IsAuchenticated = true;
            Auchenticated?.Invoke(this, EventArgs.Empty);
            return new Commands.Authenticate.Response();
        }
        catch (Exception ex)
        {
            cts?.Cancel();
            LastException = ex;
            Disconnect();
            _ = Task.Delay(1000).ContinueWith(t => Stop());
            throw;
        }
    }

    private Commands.HandShake.Response handShake(QpChannel handler, Commands.HandShake.Request request)
    {
        Options.CommandExecuterManagerList.AddRange(authedCommandExecuterManagerList);
        Options.NoticeHandlerManagerList = authedNoticeHandlerManagerList;
        Options.InternalCompress = request.EnableCompress;
        Options.InternalEncrypt = request.EnableEncrypt;
        Options.InternalTransportTimeout = request.TransportTimeout;

        //改变传输超时时间
        ChangeTransportTimeout();

        //开始心跳
        if (Options.HeartBeatInterval > 0)
            _ = BeginHeartBeat(cts.Token);
        return new Commands.HandShake.Response();
    }

    private Commands.GetQpInstructions.Response getQpInstructions(QpChannel handler, Commands.GetQpInstructions.Request request)
    {
        return new Commands.GetQpInstructions.Response()
        {
            Data = Options.InstructionSet
        };
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        try
        {
            cts?.Cancel();
            stream?.Dispose();
        }
        catch { }
    }

    protected override void OnWriteError(Exception exception)
    {
        Stop();
        base.OnWriteError(exception);
    }

    protected override void OnReadError(Exception exception)
    {
        if (Options.ProtocolErrorHandler != null)
        {
            if (exception is ProtocolException protocolException)
            {
                if (Options.Logger is { LogConnection: true })
                    Options.Logger.Log("[ProtocolErrorHandler]{0}: Begin ProtocolErrorHandler invoke...", DateTime.Now);

                Options.ProtocolErrorHandler.Invoke(stream, protocolException.ReadBuffer);
                return;
            }
        }
        Stop();
        base.OnReadError(exception);
    }
}