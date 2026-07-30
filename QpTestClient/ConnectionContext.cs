using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QpTestClient
{
    public class ConnectionContext : IDisposable
    {
        public TestConnectionInfo ConnectionInfo { get; private set; }

        private bool _Connected = false;
        public bool Connected
        {
            get { return _Connected; }
            private set
            {
                if (_Connected == value)
                    return;
                _Connected = value;
                if (!value)
                    Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        public QpClient QpClient { get; private set; }
        public event EventHandler Disconnected;

        public ConnectionContext(TestConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
        }

        public async Task Connect()
        {
            var qpClientTypeInfo = QpClientTypeManager.Instance.Get(ConnectionInfo.QpClientTypeName);
            if (qpClientTypeInfo == null)
                throw new ApplicationException($"未找到类型为[{ConnectionInfo.QpClientTypeName}]的QP客户端类型！");

            //替换基础架构
            if (ConnectionInfo.QpClientOptions.InstructionSet != null)
            {
                for (var i = 0; i < ConnectionInfo.QpClientOptions.InstructionSet.Length; i++)
                {
                    var item = ConnectionInfo.QpClientOptions.InstructionSet[i];
                    if (item.Id == Base.Instruction.Id)
                    {
                        ConnectionInfo.QpClientOptions.InstructionSet[i] = Base.Instruction;
                        break;
                    }
                }
            }
            QpClient = ConnectionInfo.QpClientOptions.CreateClient();
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
