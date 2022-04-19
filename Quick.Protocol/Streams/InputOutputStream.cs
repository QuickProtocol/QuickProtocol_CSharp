using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.Protocol.Streams
{
    public class InputOutputStream : Stream
    {
        private Stream input;
        private Stream output;
        public InputOutputStream(Stream input, Stream output)
        {
            this.input = input;
            this.output = output;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            output.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return input.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            output.Write(buffer, offset, count);
        }
    }
}
