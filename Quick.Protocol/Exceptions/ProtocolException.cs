using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.Exceptions
{
    public class ProtocolException : Exception
    {
        public ArraySegment<byte> ReadBuffer { get; set; }
        public ProtocolException(ArraySegment<byte> readBuffer, string message)
            : base(message)
        {
            ReadBuffer = readBuffer;
        }
    }
}
