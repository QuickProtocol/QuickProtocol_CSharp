using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QpTestClient
{
    public class ConnectionContext : IDisposable
    {
        public ConnectionInfo ConnectionInfo { get; private set; }
        public bool Connected { get; private set; } = false;
        public QpClient QpClient { get; private set; }

        public ConnectionContext(ConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
        }

        public async Task Connect()
        {
            var qpClientTypeInfo = QpClientTypeManager.Instance.Get(ConnectionInfo.QpClientTypeName);
            if (qpClientTypeInfo == null)
                throw new ApplicationException($"未找到类型为[{ConnectionInfo.QpClientTypeName}]的QP客户端类型！");
            QpClient = (QpClient)Activator.CreateInstance(qpClientTypeInfo.QpClientType, ConnectionInfo.QpClientOptions);
            QpClient.Disconnected += QpClient_Disconnected;
            try
            {
                await QpClient.ConnectAsync();
            }
            catch
            {
                QpClient?.Close();
                QpClient = null;
                throw;
            }
            //获取指令集信息
            try
            {
                var rep = await QpClient.SendCommand(new Quick.Protocol.Commands.GetQpInstructions.Request());
                ConnectionInfo.Instructions = rep.Data;
                Connected = true;
            }
            catch
            {
                QpClient?.Close();
                QpClient = null;
                throw;
            }
        }

        private void QpClient_Disconnected(object sender, EventArgs e)
        {
            Connected = false;
        }

        public void Dispose()
        {
            Connected = false;
            var client = QpClient;
            QpClient = null;
            client?.Close();
        }
    }
}
