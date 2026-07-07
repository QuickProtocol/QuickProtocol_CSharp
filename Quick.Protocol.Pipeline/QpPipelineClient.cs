using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
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
            try
            {
                pipeClientStream = new NamedPipeClientStream(options.ServerName, options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await pipeClientStream.ConnectAsync(options.ConnectionTimeout).ConfigureAwait(false);
            }
            catch
            {
                try 
                {
                    pipeClientStream?.Close();
                    pipeClientStream?.Dispose();
                }
                catch { }
                pipeClientStream = null;
                throw;
            }
            return pipeClientStream;
        }

        public override void Dispose()
        {
            pipeClientStream?.Close();
            pipeClientStream?.Dispose();
            pipeClientStream = null;
            base.Dispose();
        }
    }
}
