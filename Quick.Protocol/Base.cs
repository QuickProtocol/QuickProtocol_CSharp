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
                QpNoticeInfo.Create<Notices.PrivateNotice>(Notices.NoticesSerializerContext.Default)
            },
            CommandInfos = new QpCommandInfo[]
            {
                QpCommandInfo.Create(
                    new Commands.Connect.Request(),
                    Commands.ConnectCommandSerializerContext.Default),
                QpCommandInfo.Create(
                    new Commands.Authenticate.Request(),
                    Commands.AuthenticateCommandSerializerContext.Default),
                QpCommandInfo.Create(
                    new Commands.HandShake.Request(),
                    Commands.HandShakeCommandSerializerContext.Default),
                QpCommandInfo.Create(
                    new Commands.PrivateCommand.Request(),
                    Commands.PrivateCommandCommandSerializerContext.Default),
                QpCommandInfo.Create(
                    new Commands.GetQpInstructions.Request(),
                    Commands.GetQpInstructionsCommandSerializerContext.Default)
            }
        };
    }
}
