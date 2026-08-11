namespace Quick.Protocol;

public class QpLogger
{
    private static readonly QpLogger _emptyLogger = new QpLogger(null);
    public static QpLogger EmptyLogger => _emptyLogger;

    public const string NOT_SHOW_CONTENT_MESSAGE = "[NOT_SHOW: LogContent is False]";
    public bool LogPackage { get; set; } = false;
    public bool LogHeartbeat { get; set; } = false;
    public bool LogNotice { get; set; } = false;
    public bool LogCommand { get; set; } = false;
    public bool LogContent { get; set; } = false;
    public bool LogConnection { get; set; } = false;
    public bool LogRaw { get; set; } = false;
    private Action<string> logHandler;

    public QpLogger(Action<string> logHandler)
    {
        this.logHandler = logHandler;
    }

    public void Log(string template, params object[] args)
    {
        Log(string.Format(template, args));
    }

    public void Log(string content)
    {
        logHandler?.Invoke("[QuickProtocol]"+content);
    }
}