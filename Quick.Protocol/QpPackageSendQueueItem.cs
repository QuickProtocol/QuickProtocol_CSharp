using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Protocol
{
    /// <summary>
    /// 包发送队列对象
    /// </summary>
    public class QpPackageSendQueueItem
    {
        private Exception sendException;
        public Task SendTask { get; private set; }
        public Func<byte[], ArraySegment<byte>> GetPackagePayloadFunc { get; private set; }
        public Action AfterSendHandler { get; private set; }

        public QpPackageSendQueueItem(Func<byte[], ArraySegment<byte>> getPackagePayloadFunc, Action afterSendHandler)
        {
            GetPackagePayloadFunc = getPackagePayloadFunc;
            AfterSendHandler = afterSendHandler;
            SendTask = new Task(() =>
            {
                if (sendException == null)
                    return;
                throw new IOException("Send package error.", sendException);
            });
        }

        public void SetResult(Exception ex)
        {
            sendException = ex;
            SendTask.Start();
        }
    }
}
