using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol
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
