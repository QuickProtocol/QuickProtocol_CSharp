namespace Quick.Protocol;

public class QpLogger
{
    public const string NOT_SHOW_CONTENT_MESSAGE = "[NOT_SHOW: LogUtils.LogContent is False]";
    public bool LogPackage { get; set; } = false;
    public bool LogHeartbeat { get; set; } = false;
    public bool LogNotice { get; set; } = false;
    public bool LogCommand { get; set; } = false;
    public bool LogContent { get; set; } = false;
    public bool LogConnection { get; set; } = false;
    public bool LogRaw { get; set; } = false;

    private Action<string> LogHandler = null;

    public void SetConsoleLogHandler() => SetLogHandler(Console.WriteLine);

    public void SetLogHandler(Action<string> logHandler)
    {
        LogHandler = logHandler;
    }

    public void Log(string template, params object[] args)
    {
        Log(string.Format(template, args));
    }

    public void Log(string content)
    {
        LogHandler?.Invoke(content);
    }
}