namespace Quick.Protocol;

/// <summary>
/// 命令执行器代理
/// </summary>
/// <typeparam name="TCmdRequest"></typeparam>
/// <typeparam name="TCmdResponse"></typeparam>
/// <param name="channel"></param>
/// <param name="request"></param>
/// <returns></returns>
public delegate Task<TCmdResponse> CommandExecuter<TCmdRequest, TCmdResponse>(QpChannel channel, TCmdRequest request);
/// <summary>
/// 命令执行器代理
/// </summary>
/// <param name="channel"></param>
/// <param name="request"></param>
/// <returns></returns>
public delegate Task<object> CommandExecuter(QpChannel channel, object request);

public class CommandExecuterManager
{
    private Dictionary<string, CommandExecuter> commandExecuterDict = new Dictionary<string, CommandExecuter>();

    /// <summary>
    /// 获取全部注册的命令请求类型名称
    /// </summary>
    public string[] GetRegisterCommandRequestTypeNames() => commandExecuterDict.Keys.ToArray();

    public void Register(string cmdRequestTypeName, CommandExecuter commandExecuter)
    {
        commandExecuterDict[cmdRequestTypeName] = commandExecuter;
    }

    public void Register<TCmdRequest, TCmdResponse>(CommandExecuter<TCmdRequest, TCmdResponse> commandExecuter)
        where TCmdRequest : class, new()
        where TCmdResponse : class, new()
    {
        var cmdRequestTypeName = typeof(TCmdRequest).FullName;
        commandExecuterDict[cmdRequestTypeName] = async (handler, request) => await commandExecuter(handler, (TCmdRequest)request);
    }

    public void Register<TCmdRequest, TCmdResponse>(TCmdRequest request, CommandExecuter<TCmdRequest, TCmdResponse> commandExecuter)
        where TCmdRequest : class, IQpCommandRequest<TCmdRequest, TCmdResponse>, new()
        where TCmdResponse : class, new()
    {
        var cmdRequestTypeName = request.GetType().FullName;
        commandExecuterDict[cmdRequestTypeName] = async (handler, req) => await commandExecuter(handler, (TCmdRequest)req);
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="cmdRequestTypeName"></param>
    /// <param name="cmdRequestModel"></param>
    /// <returns></returns>
    public virtual Task<object> ExecuteCommand(QpChannel handler, string cmdRequestTypeName, object cmdRequestModel)
    {
        if (!commandExecuterDict.TryGetValue(cmdRequestTypeName, out var commandExecuter))
            throw new IOException($"Command Request Type[{cmdRequestTypeName}] has no executer.");
        return commandExecuter(handler, cmdRequestModel);
    }

    /// <summary>
    /// 能否执行指定类型的命令
    /// </summary>
    /// <param name="cmdRequestTypeName"></param>
    /// <returns></returns>
    public virtual bool CanExecuteCommand(string cmdRequestTypeName)
    {
        return commandExecuterDict.ContainsKey(cmdRequestTypeName);
    }
}