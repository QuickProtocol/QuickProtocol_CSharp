namespace Quick.Protocol;

public class NoticeHandlerManager
{
    private Dictionary<string, Action<QpChannel, object>> noticeHandlerDict = new Dictionary<string, Action<QpChannel, object>>();

    /// <summary>
    /// 获取全部注册的通知类型名称
    /// </summary>
    public string[] GetRegisterNoticeTypeNames() => noticeHandlerDict.Keys.ToArray();

    public void Register(string noticeTypeName, Action<QpChannel, object> noticeHandler)
    {
        noticeHandlerDict[noticeTypeName] = noticeHandler;
    }

    public void Register<TNotice>(Action<QpChannel, TNotice> noticeHandler)
        where TNotice : class, new()
    {
        var noticeTypeName = typeof(TNotice).FullName;
        noticeHandlerDict[noticeTypeName] = (handler, notice) => noticeHandler(handler, (TNotice)notice);
    }


    /// <summary>
    /// 处理通知
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="noticeTypeName"></param>
    /// <param name="noticeModel"></param>
    /// <returns></returns>
    public virtual void HandleNotice(QpChannel handler, string noticeTypeName, object noticeModel)
    {
        if (!CanHandleNoticed(noticeTypeName))
            return;
        var noticeHandler = noticeHandlerDict[noticeTypeName];
        noticeHandler(handler, noticeModel);
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