using System.Text.Json.Serialization;

namespace Quick.Protocol
{
    [JsonSerializable(typeof(Notices.PrivateNotice))]
    internal partial class PrivateNoticeSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(Commands.Connect.Request))]
    [JsonSerializable(typeof(Commands.Connect.Response))]
    internal partial class ConnectCommandSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(Commands.Authenticate.Request))]
    [JsonSerializable(typeof(Commands.Authenticate.Response))]
    internal partial class AuthenticateCommandSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(Commands.HandShake.Request))]
    [JsonSerializable(typeof(Commands.HandShake.Response))]
    internal partial class HandShakeCommandSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(Commands.PrivateCommand.Request))]
    [JsonSerializable(typeof(Commands.PrivateCommand.Response))]
    internal partial class PrivateCommandCommandSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(Commands.GetQpInstructions.Request))]
    [JsonSerializable(typeof(Commands.GetQpInstructions.Response))]
    internal partial class GetQpInstructionsCommandCommandSerializerContext : JsonSerializerContext { }

    public class Base
    {
        public static QpInstruction Instruction => new QpInstruction()
        {
            Id = typeof(Base).FullName,
            Name = "基础指令集",
            NoticeInfos = new QpNoticeInfo[]
            {
                QpNoticeInfo.Create(PrivateNoticeSerializerContext.Default.PrivateNotice)
            },
            CommandInfos = new QpCommandInfo[]
            {
                QpCommandInfo.Create(
                    ConnectCommandSerializerContext.Default.Request,
                    ConnectCommandSerializerContext.Default.Response),
                QpCommandInfo.Create(
                    AuthenticateCommandSerializerContext.Default.Request,
                    AuthenticateCommandSerializerContext.Default.Response),
                QpCommandInfo.Create(
                    HandShakeCommandSerializerContext.Default.Request,
                    HandShakeCommandSerializerContext.Default.Response),
                QpCommandInfo.Create(
                    PrivateCommandCommandSerializerContext.Default.Request,
                    PrivateCommandCommandSerializerContext.Default.Response),
                QpCommandInfo.Create(
                    GetQpInstructionsCommandCommandSerializerContext.Default.Request,
                    GetQpInstructionsCommandCommandSerializerContext.Default.Response)
            }
        };
    }
}
