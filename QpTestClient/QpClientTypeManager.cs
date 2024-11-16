using QpTestClient.Controls.ClientOptions;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml.Linq;

namespace QpTestClient
{
    public class QpClientTypeManager
    {
        public static QpClientTypeManager Instance { get; } = new QpClientTypeManager();
        private Dictionary<string, QpClientTypeInfo> dict = null;

        private void register(QpClientTypeInfo qpClientTypeInfo)
        {
            dict[qpClientTypeInfo.TypeName] = qpClientTypeInfo;
        }

        public void Init()
        {
            dict = new Dictionary<string, QpClientTypeInfo>();
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.Tcp.QpTcpClient).FullName,
                Name = "TCP",
                CreateOptionsControlFunc = () => new Controls.ClientOptions.TcpClientOptionsControl(),
                CreateOptionsInstanceFunc = () => new Quick.Protocol.Tcp.QpTcpClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.Tcp.QpTcpClientOptionsSerializerContext.Default.QpTcpClientOptions)
            });
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.Pipeline.QpPipelineClient).FullName,
                Name = "命名管道",
                CreateOptionsControlFunc = () => new Controls.ClientOptions.PipelineClientOptionsControl(),
                CreateOptionsInstanceFunc = () => new Quick.Protocol.Pipeline.QpPipelineClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.Pipeline.QpPipelineClientOptionsSerializerContext.Default.QpPipelineClientOptions)
            });
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.SerialPort.QpSerialPortClient).FullName,
                Name = "串口",
                CreateOptionsControlFunc = () => new Controls.ClientOptions.SerialPortClientOptionsControl(),
                CreateOptionsInstanceFunc = () => new Quick.Protocol.SerialPort.QpSerialPortClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.SerialPort.QpSerialPortClientOptionsSerializerContext.Default.QpSerialPortClientOptions)
            });
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.WebSocket.Client.QpWebSocketClient).FullName,
                Name = "WebSocket",
                CreateOptionsControlFunc = () => new Controls.ClientOptions.WebSocketClientOptionsControl(),
                CreateOptionsInstanceFunc = () => new Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.WebSocket.Client.QpWebSocketClientOptionsSerializerContext.Default.QpWebSocketClientOptions)
            });
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
