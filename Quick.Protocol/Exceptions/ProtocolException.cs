using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.Exceptions
{
    public class ProtocolException : Exception
    {
        public ReadOnlySequence<byte> ReadBuffer { get; set; }
        public ProtocolException(ReadOnlySequence<byte> readBuffer, string message)
            : base(message)
        {
            ReadBuffer = readBuffer;
        }
    }
}
