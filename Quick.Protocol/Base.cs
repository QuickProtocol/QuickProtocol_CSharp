using System;
using System.Collections.Generic;
using System.Text;

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
                QpNoticeInfo.Create<Notices.PrivateNotice>()
            },
            CommandInfos = new QpCommandInfo[]
            {
                QpCommandInfo.Create(new Commands.Connect.Request()),
                QpCommandInfo.Create(new Commands.Authenticate.Request()),
                QpCommandInfo.Create(new Commands.HandShake.Request()),
                QpCommandInfo.Create(new Commands.PrivateCommand.Request()),
                QpCommandInfo.Create(new Commands.GetQpInstructions.Request()),
            }
        };
    }
}
