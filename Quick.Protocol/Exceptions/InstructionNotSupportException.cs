using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.Protocol.Exceptions
{
    public class InstructionNotSupportException : IOException
    {
        public InstructionNotSupportException(string message) : base(message)
        {
        }
    }
}
