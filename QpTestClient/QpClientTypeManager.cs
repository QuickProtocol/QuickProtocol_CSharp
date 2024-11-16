using QpTestClient.Controls;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private void EditCommonClientOptions(AotPropertyGrid propertyGrid, QpClientOptions options)
        {
            propertyGrid.RegisterProperty("密码", "", () => options.Password, t => options.Password = t);
            propertyGrid.RegisterGroup("高级");
            propertyGrid.RegisterProperty("连接超时", "单位：秒", () => options.ConnectionTimeout, t => options.ConnectionTimeout = t);
            propertyGrid.RegisterProperty("传输超时", "单位：秒", () => options.TransportTimeout, t => options.TransportTimeout = t);
            propertyGrid.RegisterProperty("加密", "", () => options.EnableEncrypt, t => options.EnableEncrypt = t);
            propertyGrid.RegisterProperty("压缩", "", () => options.EnableCompress, t => options.EnableCompress = t);
            propertyGrid.RegisterProperty("最大包大小", "单位：字节", () => options.MaxPackageSize, t => options.MaxPackageSize = t);
            propertyGrid.RegisterProperty("网络统计", "", () => options.EnableNetstat, t => options.EnableNetstat = t);
            propertyGrid.RegisterProperty("通知接收事件", "", () => options.RaiseNoticePackageReceivedEvent, t => options.RaiseNoticePackageReceivedEvent = t);
        }

        public void Init()
        {
            dict = new Dictionary<string, QpClientTypeInfo>();
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.Tcp.QpTcpClient).FullName,
                Name = "TCP",
                CreateOptionsInstanceFunc = () => new Quick.Protocol.Tcp.QpTcpClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.Tcp.QpTcpClientOptionsSerializerContext.Default.QpTcpClientOptions),
                EditOptions = (propertyGrid, options) =>
                {
                    var tcpClientOptions = (Quick.Protocol.Tcp.QpTcpClientOptions)options;
                    propertyGrid.RegisterGroup("常用");
                    propertyGrid.RegisterProperty("主机", "", () => tcpClientOptions.Host, t => tcpClientOptions.Host = t);
                    propertyGrid.RegisterProperty("端口", "", () => tcpClientOptions.Port, t => tcpClientOptions.Port = t);
                    EditCommonClientOptions(propertyGrid, options);
                    propertyGrid.RegisterProperty("本地主机", "", () => tcpClientOptions.LocalHost, t => tcpClientOptions.LocalHost = t);
                    propertyGrid.RegisterProperty("本地端口", "", () => tcpClientOptions.LocalPort, t => tcpClientOptions.LocalPort = t);
                }
            });
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.Pipeline.QpPipelineClient).FullName,
                Name = "命名管道",
                CreateOptionsInstanceFunc = () => new Quick.Protocol.Pipeline.QpPipelineClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.Pipeline.QpPipelineClientOptionsSerializerContext.Default.QpPipelineClientOptions),
                EditOptions = (propertyGrid, options) =>
                {
                    var pipelineClientOptions = (Quick.Protocol.Pipeline.QpPipelineClientOptions)options;
                    propertyGrid.RegisterGroup("常用");
                    propertyGrid.RegisterProperty("服务器名称", "", () => pipelineClientOptions.ServerName, t => pipelineClientOptions.ServerName = t);
                    propertyGrid.RegisterProperty("管道名称", "", () => pipelineClientOptions.PipeName, t => pipelineClientOptions.PipeName = t);
                    EditCommonClientOptions(propertyGrid, options);
                }
            });
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.SerialPort.QpSerialPortClient).FullName,
                Name = "串口",
                CreateOptionsInstanceFunc = () => new Quick.Protocol.SerialPort.QpSerialPortClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.SerialPort.QpSerialPortClientOptionsSerializerContext.Default.QpSerialPortClientOptions),
                EditOptions = (propertyGrid, options) =>
                {
                    var serialPortClientOptions = (Quick.Protocol.SerialPort.QpSerialPortClientOptions)options;
                    propertyGrid.RegisterGroup("常用");
                    propertyGrid.RegisterProperty("端口名称", "", () => serialPortClientOptions.PortName, t => serialPortClientOptions.PortName = t);
                    propertyGrid.RegisterProperty("波特率", "", () => serialPortClientOptions.BaudRate, t => serialPortClientOptions.BaudRate = t);
                    propertyGrid.RegisterProperty("奇偶校验位", "", () => serialPortClientOptions.Parity, t => serialPortClientOptions.Parity = t);
                    propertyGrid.RegisterProperty("数据位", "", () => serialPortClientOptions.DataBits, t => serialPortClientOptions.DataBits = t);
                    propertyGrid.RegisterProperty("停止位", "", () => serialPortClientOptions.StopBits, t => serialPortClientOptions.StopBits = t);
                    EditCommonClientOptions(propertyGrid, options);
                }
            });
            register(new QpClientTypeInfo()
            {
                TypeName = typeof(Quick.Protocol.WebSocket.Client.QpWebSocketClient).FullName,
                Name = "WebSocket",
                CreateOptionsInstanceFunc = () => new Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions(),
                DeserializeQpClientOptions = stream => JsonSerializer.Deserialize(stream, Quick.Protocol.WebSocket.Client.QpWebSocketClientOptionsSerializerContext.Default.QpWebSocketClientOptions),
                EditOptions = (propertyGrid, options) =>
                {
                    var websocketClientOptions = (Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions)options;
                    propertyGrid.RegisterGroup("常用");
                    propertyGrid.RegisterProperty("URL", "", () => websocketClientOptions.Url, t => websocketClientOptions.Url = t);
                    EditCommonClientOptions(propertyGrid, options);
                }
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
