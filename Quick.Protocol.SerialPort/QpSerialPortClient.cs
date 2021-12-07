using Quick.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Protocol.SerialPort
{
    [DisplayName("串口")]
    public class QpSerialPortClient : QpClient
    {
        private QpSerialPortClientOptions options;
        private System.IO.Ports.SerialPort serialPort;
        public QpSerialPortClient(QpSerialPortClientOptions options) : base(options)
        {
            this.options = options;
        }

        protected override async Task<Stream> InnerConnectAsync()
        {
            if (LogUtils.LogConnection)
                Console.WriteLine($"Opening SerialPort[{options.PortName}]...");
            serialPort = new System.IO.Ports.SerialPort(options.PortName,
                                                            options.BaudRate,
                                                            options.Parity,
                                                            options.DataBits,
                                                            options.StopBits);
            await Task.Run(() => serialPort.Open());
            if (LogUtils.LogConnection)
                Console.WriteLine($"SerialPort[{options.PortName}] open success.");
            serialPort.WriteTimeout = options.TransportTimeout;
            serialPort.WriteLine(QpConsts.QuickProtocolNameAndVersion);
            return serialPort.BaseStream;
        }

        protected override void Disconnect()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
                serialPort = null;
            }
            base.Disconnect();
        }
    }
}
