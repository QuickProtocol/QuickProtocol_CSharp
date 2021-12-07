using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.Pipeline
{
    public class QpPipelineServer : QpServer
    {
        private QpPipelineServerOptions options;
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

        protected override Task InnerAcceptAsync(CancellationToken token)
        {
            var serverStream = new NamedPipeServerStream(options.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Task waitForConnectionTask = null;
#if NETSTANDARD2_0
            waitForConnectionTask = serverStream.WaitForConnectionAsync(token);
#else
            waitForConnectionTask = Task.Run(() => serverStream.WaitForConnection());
#endif
            return waitForConnectionTask.ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                if (task.IsFaulted)
                    return;
                OnNewChannelConnected(serverStream, $"Pipe:{options.PipeName}", token);
            });
        }
    }
}
