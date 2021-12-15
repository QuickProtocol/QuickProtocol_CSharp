using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.Protocol.AllClients
{
    public class ConnectionInfo
    {
        public Type QpClientType { get; set; }
        public QpClientOptions QpClientOptions { get; set; }

        public QpClient CreateInstance()
        {
            var client = (QpClient)Activator.CreateInstance(QpClientType, QpClientOptions);
            return client;
        }
    }
}
