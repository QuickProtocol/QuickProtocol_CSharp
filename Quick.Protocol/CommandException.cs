using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol
{
    /// <summary>
    /// 命令异常
    /// </summary>
    public class CommandException : Exception
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public byte Code { get; set; }
        public CommandException(byte code, string message)
            : base(message)
        {
            if (code == 0)
                throw new Exception("Code in CommandException must bigger than 0.");
            Code = code;
        }

        public CommandException(byte code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }
    }
}
