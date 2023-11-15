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

        private void register<TClient>()
        {
            register(typeof(TClient));
        }

        private void register(Type type)
        {
            var typeConstructor = type.GetConstructors()[0];
            var typeConstructorParameters = typeConstructor.GetParameters();
            if (typeConstructorParameters == null || typeConstructorParameters.Length != 1)
                return;
            var optionsType = typeConstructorParameters[0].ParameterType;
            var name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.Name;
            dict[type.FullName] = new QpClientTypeInfo()
            {
                Name = name,
                QpClientType = type,
                QpClientOptionsType = optionsType
            };
        }

        public void Init()
        {
            dict = new Dictionary<string, QpClientTypeInfo>();
            register<Quick.Protocol.Tcp.QpTcpClient>();
            register<Quick.Protocol.Udp.QpUdpClient>();
            register<Quick.Protocol.Pipeline.QpPipelineClient>();
            register<Quick.Protocol.SerialPort.QpSerialPortClient>();
            register<Quick.Protocol.WebSocket.Client.QpWebSocketClient>();
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
