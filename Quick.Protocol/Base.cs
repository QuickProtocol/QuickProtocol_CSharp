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
                    Commands.Connect.Request.GetDefine(),
                    Commands.Connect.Response.GetDefine()),
                QpCommandInfo.Create(
                    Commands.Authenticate.Request.GetDefine(),
                    Commands.Authenticate.Response.GetDefine()),
                QpCommandInfo.Create(
                    Commands.HandShake.Request.GetDefine(),
                    Commands.HandShake.Response.GetDefine()),
                QpCommandInfo.Create(
                    Commands.PrivateCommand.Request.GetDefine(),
                    Commands.PrivateCommand.Response.GetDefine()),
                QpCommandInfo.Create(
                    Commands.GetQpInstructions.Request.GetDefine(),
                    Commands.GetQpInstructions.Response.GetDefine())
            }
        };
    }
}
