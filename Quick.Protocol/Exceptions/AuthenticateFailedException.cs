using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.Protocol.Exceptions
{
    /// <summary>
    /// 认证失败异常
    /// </summary>
    public class AuthenticateFailedException : IOException
    {
        public AuthenticateFailedException(string message) : base(message)
        {
        }
    }
}
