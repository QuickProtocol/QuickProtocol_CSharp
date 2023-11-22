using Quick.Protocol.Commands;
using Quick.Protocol.Notices;

namespace Quick.Protocol
{
    public class Base
    {
        public static QpInstruction Instruction => new QpInstruction()
        {
            Id = typeof(Base).FullName,
            Name = "基础指令集",
            NoticeInfos = new QpNoticeInfo[]
            {
                QpNoticeInfo.Create(NoticesSerializerContext.Default.PrivateNotice)
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
