using Quick.Protocol.Pipeline;
using Quick.Protocol.SerialPort;
using Quick.Protocol.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient.Controls.ClientOptions
{
    public partial class SerialPortClientOptionsControl : UserControl
    {
        public SerialPortClientOptionsControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, EventArgs e)
        {
            var options = (QpSerialPortClientOptions)DataContext;

            ClientOptionsControlUtils.LinkControl(lblPortName, txtPortName);
            ClientOptionsControlUtils.BindString(txtPortName, () => options.PortName, t => options.PortName = t);

            ClientOptionsControlUtils.LinkControl(lblBaudRate, txtBaudRate);
            ClientOptionsControlUtils.BindInt32(txtBaudRate, () => options.BaudRate, t => options.BaudRate = t);

            ClientOptionsControlUtils.LinkControl(lblParity, cbParity);
            ClientOptionsControlUtils.BindEnum(cbParity, () => options.Parity, t => options.Parity = t);

            ClientOptionsControlUtils.LinkControl(lblDataBits, txtDataBits);
            ClientOptionsControlUtils.BindInt32(txtDataBits, () => options.DataBits, t => options.DataBits = t);

            ClientOptionsControlUtils.LinkControl(lblStopBits, cbStopBits);
            ClientOptionsControlUtils.BindEnum(cbStopBits, () => options.StopBits, t => options.StopBits = t);

            ClientOptionsControlUtils.LinkControl(lblPassword, txtPassword);
            ClientOptionsControlUtils.BindString(txtPassword, () => options.Password, t => options.Password = t);

            ClientOptionsControlUtils.LinkControl(lblConnectionTimeout, txtConnectionTimeout);
            ClientOptionsControlUtils.BindInt32(txtConnectionTimeout, () => options.ConnectionTimeout, t => options.ConnectionTimeout = t);

            ClientOptionsControlUtils.LinkControl(lblTransportTimeout, txtTransportTimeout);
            ClientOptionsControlUtils.BindInt32(txtTransportTimeout, () => options.TransportTimeout, t => options.TransportTimeout = t);

            ClientOptionsControlUtils.LinkControl(lblEnableEncrypt, cbEnableEncrypt);
            ClientOptionsControlUtils.BindBoolean(cbEnableEncrypt, () => options.EnableEncrypt, t => options.EnableEncrypt = t);

            ClientOptionsControlUtils.LinkControl(lblEnableCompress, cbEnableCompress);
            ClientOptionsControlUtils.BindBoolean(cbEnableCompress, () => options.EnableCompress, t => options.EnableCompress = t);

            ClientOptionsControlUtils.LinkControl(lblMaxPackageSize, txtMaxPackageSize);
            ClientOptionsControlUtils.BindInt32(txtMaxPackageSize, () => options.MaxPackageSize, t => options.MaxPackageSize = t);

            ClientOptionsControlUtils.LinkControl(lblEnableNetstat, cbEnableNetstat);
            ClientOptionsControlUtils.BindBoolean(cbEnableNetstat, () => options.EnableNetstat, t => options.EnableNetstat = t);

            ClientOptionsControlUtils.LinkControl(lblRaiseNoticePackageReceivedEvent, cbRaiseNoticePackageReceivedEvent);
            ClientOptionsControlUtils.BindBoolean(cbRaiseNoticePackageReceivedEvent, () => options.RaiseNoticePackageReceivedEvent, t => options.RaiseNoticePackageReceivedEvent = t);
        }
    }
}
