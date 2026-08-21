namespace Quick.Protocol;

/// <summary>
/// 通知处理器代理
/// </summary>
/// <typeparam name="TNotice"></typeparam>
/// <param name="channel"></param>
/// <param name="notice"></param>
/// <returns></returns>
public delegate ValueTask NoticeHandler<TNotice>(QpChannel channel, TNotice notice);
/// <summary>
/// 通知处理器代理
/// </summary>
/// <param name="channel"></param>
/// <param name="request"></param>
/// <returns></returns>
public delegate ValueTask NoticeHandler(QpChannel channel, object notice);

public class NoticeHandlerManager
{
    private Dictionary<string, NoticeHandler> noticeHandlerDict = new Dictionary<string, NoticeHandler>();

    /// <summary>
    /// 获取全部注册的通知类型名称
    /// </summary>
    public string[] GetRegisterNoticeTypeNames() => noticeHandlerDict.Keys.ToArray();

    public void Register(string noticeTypeName, NoticeHandler noticeHandler)
    {
        noticeHandlerDict[noticeTypeName] = noticeHandler;
    }

    public void Register<TNotice>(NoticeHandler<TNotice> noticeHandler)
        where TNotice : class, new()
    {
        var noticeTypeName = typeof(TNotice).FullName;
        noticeHandlerDict[noticeTypeName] = async (handler, notice) => await noticeHandler(handler, (TNotice)notice);
    }


    /// <summary>
    /// 处理通知
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="noticeTypeName"></param>
    /// <param name="noticeModel"></param>
    /// <returns></returns>
    public virtual async ValueTask HandleNotice(QpChannel handler, string noticeTypeName, object noticeModel)
    {
        if (noticeHandlerDict.TryGetValue(noticeTypeName, out var noticeHandler))
            await noticeHandler(handler, noticeModel);
    }

    /// <summary>
    /// 能否处理指定类型的通知
    /// </summary>
    /// <param name="noticeTypeName"></param>
    /// <returns></returns>
    public virtual bool CanHandleNoticed(string noticeTypeName)
    {
        return noticeHandlerDict.ContainsKey(noticeTypeName);
    }
}