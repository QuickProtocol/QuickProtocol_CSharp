using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol.Commands.Authenticate
{
    [DisplayName("认证")]
    public class Request : IQpCommandRequest<Response>
    {
        /// 认证回答
        /// </summary>
        public string Answer { get; set; }
    }
}
