using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Quick.Protocol.Streams
{
    public class QpStreamServerOptions : QpServerOptions
    {
        public Stream BaseStream { get; set; }
        public string ChannelName { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
