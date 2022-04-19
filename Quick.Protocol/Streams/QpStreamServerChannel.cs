using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Quick.Protocol.Streams
{
    public class QpStreamServerChannel : QpServerChannel
    {
        public QpStreamServerChannel(QpStreamServerOptions options)
            : base(options.BaseStream, options.ChannelName, options.CancellationToken, options)
        {
        }
    }
}
