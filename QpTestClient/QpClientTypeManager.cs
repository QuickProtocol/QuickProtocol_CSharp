using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace QpTestClient
{
    public class QpClientTypeManager
    {
        public static QpClientTypeManager Instance { get; } = new QpClientTypeManager();
        private Dictionary<string, QpClientTypeInfo> dict = null;

        private void register(Type clientType, Func<QpClientOptions> createOptionsInstanceFunc, Func<Control> createOptionsControlFunc)
        {
            var clientTypeFullName = clientType.FullName;
            var name = clientType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? clientTypeFullName;
            dict[clientTypeFullName] = new QpClientTypeInfo()
            {
                Name = name,
                ClientType = clientType,
                CreateOptionsInstanceFunc = createOptionsInstanceFunc,
                CreateOptionsControlFunc = createOptionsControlFunc
            };
        }

        public void Init()
        {
            dict = new Dictionary<string, QpClientTypeInfo>();
            register(typeof(Quick.Protocol.Tcp.QpTcpClient), () => new Quick.Protocol.Tcp.QpTcpClientOptions(), () => new Controls.ClientOptions.TcpClientOptionsControl());
            register(typeof(Quick.Protocol.Pipeline.QpPipelineClient), () => new Quick.Protocol.Pipeline.QpPipelineClientOptions(),()=>new Controls.ClientOptions.PipelineClientOptionsControl());
            //register(typeof(Quick.Protocol.SerialPort.QpSerialPortClient), () => new Quick.Protocol.SerialPort.QpSerialPortClientOptions());
            //register(typeof(Quick.Protocol.WebSocket.Client.QpWebSocketClient), () => new Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions());
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
