using System;
using System.IO.Ports;
using System.Text.Json.Serialization;

namespace Quick.Protocol.SerialPort
{
    [JsonSerializable(typeof(QpSerialPortServerOptions))]
    public partial class QpSerialPortServerOptionsOptionsSerializerContext : JsonSerializerContext { }

    public class QpSerialPortServerOptions : QpServerOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpSerialPortServerOptionsOptionsSerializerContext.Default;

        /// <summary>
        /// 端口名称
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; internal set; } = 9600;
        /// <summary>
        /// 奇偶校验位
        /// </summary>
        public Parity Parity { get; internal set; } = Parity.None;
        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits { get; internal set; } = 8;
        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; internal set; } = StopBits.One;

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(PortName))
                throw new ArgumentNullException(nameof(PortName));
        }

        public override QpServer CreateServer()
        {
            return new QpSerialPortServer(this);
        }
    }
}
