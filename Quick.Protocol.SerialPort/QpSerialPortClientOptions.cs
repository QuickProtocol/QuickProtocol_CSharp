using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;

namespace Quick.Protocol.SerialPort
{
    public class QpSerialPortClientOptions : QpClientOptions
    {
        /// <summary>
        /// 端口名称
        /// </summary>
        [DisplayName("端口名称")]
        [Category("常用")]
        public string PortName { get; set; } = "COM1";
        /// <summary>
        /// 波特率
        /// </summary>
        [DisplayName("波特率")]
        [Category("常用")]
        public int BaudRate { get; set; } = 9600;
        /// <summary>
        /// 奇偶校验位
        /// </summary>
        [DisplayName("奇偶校验位")]
        [Category("常用")]
        public Parity Parity { get; set; } = Parity.None;
        /// <summary>
        /// 数据位
        /// </summary>
        [DisplayName("数据位")]
        [Category("常用")]
        public int DataBits { get; set; } = 8;
        /// <summary>
        /// 停止位
        /// </summary>
        [DisplayName("停止位")]
        [Category("常用")]
        public StopBits StopBits { get; set; } = StopBits.One;

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(PortName))
                throw new ArgumentNullException(nameof(PortName));
        }

        public override string GetConnectionInfo() => PortName;
    }
}
