using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Pipeline
{
    public class QpPipelineServer : QpServer
    {
        private QpPipelineServerOptions options;
        public override string BindingPath => $"{QpPipelineClientOptions.URI_SCHEMA}://./{options.PipeName}";
        public QpPipelineServer(QpPipelineServerOptions options) : base(options)
        {
            this.options = options;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override async Task InnerAcceptAsync(CancellationToken token)
        {
            try
            {
                var serverStream = new NamedPipeServerStream(options.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await serverStream.WaitForConnectionAsync(token);
                OnNewChannelConnected(serverStream, $"{QpPipelineClientOptions.URI_SCHEMA}://./{options.PipeName}", token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
