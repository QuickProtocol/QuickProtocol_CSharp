using System;

namespace Quick.Protocol
{
    public class Base
    {
        public static QpInstruction Instruction { get; } = new QpInstruction()
        {
            Id = typeof(Base).FullName,
            Name = "基础指令集",
            NoticeInfos = new QpNoticeInfo[]
            {
                QpNoticeInfo.Create(new Notices.PrivateNotice(){ Action="Action", Content="Content" })
            },
            CommandInfos = new QpCommandInfo[]
            {
                QpCommandInfo.Create(
                    new Commands.Connect.Request(){ InstructionIds=new[]{ typeof(Base).FullName } },
                    new Commands.Connect.Response(){ Question=Guid.NewGuid().ToString("N") }),
                QpCommandInfo.Create(
                    new Commands.Authenticate.Request(){ Answer=Guid.NewGuid().ToString("N") },
                    new Commands.Authenticate.Response()),
                QpCommandInfo.Create(new Commands.HandShake.Request()),
                QpCommandInfo.Create(
                    new Commands.PrivateCommand.Request(){ Action="Action", Content="Content" },
                    new Commands.PrivateCommand.Response(){ Content="Content" }),
                QpCommandInfo.Create(new Commands.GetQpInstructions.Request())
            }
        };
    }
}
