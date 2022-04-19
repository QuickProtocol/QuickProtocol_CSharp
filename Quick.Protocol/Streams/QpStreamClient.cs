using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Protocol.Streams
{
    public class QpStreamClient : QpClient
    {
        private QpStreamClientOptions options;
        public QpStreamClient(QpStreamClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override Task<Stream> InnerConnectAsync()
        {
            return Task.FromResult(options.BaseStream);
        }
    }
}
