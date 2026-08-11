using System.ComponentModel;
using System.Text.Json.Serialization;
using Quick.Protocol.JsonConverters;

namespace Quick.Protocol;

public abstract class QpChannelOptions
{
    /// <summary>
    /// 类型信息
    /// </summary>
    protected abstract JsonSerializerContext GetJsonSerializerContext();

    [Browsable(false)]
    [JsonIgnore]
    public QpLogger Logger { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Category("常用")]
    [DisplayName("密码")]
    [PasswordPropertyText(true)]
    public string Password { get; set; } = "HelloQP";

    private QpInstruction[] _InstructionSet = [Base.Instruction];

    /// <summary>
    /// 支持的指令集
    /// </summary>
    [Browsable(false)]
    [JsonIgnore]
    public QpInstruction[] InstructionSet
    {
        get { return _InstructionSet; }
        set
        {
            _InstructionSet = value;
            //必须加上QP基础指令集
            if (_InstructionSet.All(t => t.Id != Base.Instruction.Id))
                _InstructionSet = new[] { Base.Instruction }
                    .Concat(_InstructionSet)
                    .ToArray();
        }
    }

    /// <summary>
    /// 最大包大小(默认为：10MB)
    /// </summary>
    [Category("高级")]
    [DisplayName("最大包大小")]
    [JsonConverter(typeof(QpJsonInt32Converter))]
    public int MaxPackageSize { get; set; } = 10 * 1024 * 1024;

    [Category("高级")]
    [DisplayName("是否启用网络统计")]
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool EnableNetstat { get; set; } = true;

    public virtual void Check()
    {
        if (Password == null)
            throw new ArgumentNullException(nameof(Password));
    }

    /// <summary>
    /// 是否触发NoticePackageReceived事件
    /// </summary>
    [Category("高级")]
    [DisplayName("是否触发通知数据包接收事件")]
    [JsonConverter(typeof(QpJsonBoolConverter))]
    public bool RaiseNoticePackageReceivedEvent { get; set; } = true;

    /// <summary>
    /// 指令执行器管理器列表
    /// </summary>
    [Browsable(false)]
    [JsonIgnore]
    public List<CommandExecuterManager> CommandExecuterManagerList { get; set; }

    /// <summary>
    /// 通知处理器管理器列表
    /// </summary>
    [Browsable(false)]
    [JsonIgnore]
    public List<NoticeHandlerManager> NoticeHandlerManagerList { get; set; }

    public void RegisterCommandExecuterManager(CommandExecuterManager manager)
    {
        if (CommandExecuterManagerList == null)
            CommandExecuterManagerList = new List<CommandExecuterManager>();
        CommandExecuterManagerList.Add(manager);
    }

    public void RegisterNoticeHandlerManager(NoticeHandlerManager manager)
    {
        if (NoticeHandlerManagerList == null)
            NoticeHandlerManagerList = new List<NoticeHandlerManager>();
        NoticeHandlerManagerList.Add(manager);
    }
}
