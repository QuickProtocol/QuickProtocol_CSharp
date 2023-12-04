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
        private readonly QpPipelineClientOptions options;
        private NamedPipeClientStream pipeClientStream;
        public QpPipelineClient(QpPipelineClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            pipeClientStream = new NamedPipeClientStream(options.ServerName, options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await pipeClientStream.ConnectAsync(options.ConnectionTimeout).ConfigureAwait(false);
            }
            catch
            {
                pipeClientStream.Dispose();
                throw;
            }
            pipeClientStream.ReadMode = PipeTransmissionMode.Byte;
            return pipeClientStream;
        }

        public override void Disconnect()
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
