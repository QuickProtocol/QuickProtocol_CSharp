using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace QpTestClient
{
    public class QpClientTypeManager
    {
        public static QpClientTypeManager Instance { get; } = new QpClientTypeManager();
        private Dictionary<string, QpClientTypeInfo> dict = null;

        private void register(Type clientType, Type clientOptionsType)
        {
            var clientTypeFullName = clientType.FullName;
            var name = clientType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? clientTypeFullName;
            dict[clientTypeFullName] = new QpClientTypeInfo()
            {
                Name = name,
                QpClientType = clientType,
                QpClientOptionsType = clientOptionsType
            };
        }

        public void Init()
        {
            dict = new Dictionary<string, QpClientTypeInfo>();
            register(typeof(Quick.Protocol.Tcp.QpTcpClient),typeof(Quick.Protocol.Tcp.QpTcpClientOptions));
            register(typeof(Quick.Protocol.Pipeline.QpPipelineClient), typeof(Quick.Protocol.Pipeline.QpPipelineClientOptions));
            register(typeof(Quick.Protocol.SerialPort.QpSerialPortClient), typeof(Quick.Protocol.SerialPort.QpSerialPortClientOptions));
            register(typeof(Quick.Protocol.WebSocket.Client.QpWebSocketClient), typeof(Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions));
        }

        public QpClientTypeInfo Get(string qpClientTypeName)
        {
            if (dict.ContainsKey(qpClientTypeName))
                return dict[qpClientTypeName];
            return null;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public QpClientTypeInfo[] GetAll() => dict.Values.ToArray();
    }
}
