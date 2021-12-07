using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Protocol.Pipeline
{
    [DisplayName("命名管道")]
    public class QpPipelineClient : QpClient
    {
        private QpPipelineClientOptions options;
        private NamedPipeClientStream pipeClientStream;
        public QpPipelineClient(QpPipelineClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            pipeClientStream = new NamedPipeClientStream(options.ServerName, options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
#if NETSTANDARD2_0
            await pipeClientStream.ConnectAsync(options.ConnectionTimeout);
#else
            await Task.Run(() => pipeClientStream.Connect(options.ConnectionTimeout));
#endif
            pipeClientStream.ReadMode = PipeTransmissionMode.Byte;
            return pipeClientStream;
        }

        protected override void Disconnect()
        {
            if (pipeClientStream != null)
            {
                pipeClientStream.Close();
                pipeClientStream.Dispose();
                pipeClientStream = null;
            }
            base.Disconnect();
        }
    }
}
