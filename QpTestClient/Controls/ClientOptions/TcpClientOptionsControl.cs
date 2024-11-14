using Quick.Protocol.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QpTestClient.Controls.ClientOptions
{
    public partial class TcpClientOptionsControl : UserControl
    {
        public TcpClientOptionsControl()
        {
            InitializeComponent();
        }

        private void TcpClientOptionsControl_DataContextChanged(object sender, EventArgs e)
        {
            var options = (QpTcpClientOptions)DataContext;

            ClientOptionsControlUtils.LinkControl(lblHost, txtHost);
            ClientOptionsControlUtils.BindString(txtHost, () => options.Host, t => options.Host = t);

            ClientOptionsControlUtils.LinkControl(lblPort, txtPort);
            ClientOptionsControlUtils.BindInt32(txtPort, () => options.Port, t => options.Port = t);

            ClientOptionsControlUtils.LinkControl(lblPassword, txtPassword);
            ClientOptionsControlUtils.BindString(txtPassword, () => options.Password, t => options.Password = t);

            ClientOptionsControlUtils.LinkControl(lblLocalHost, txtLocalHost);
            ClientOptionsControlUtils.BindString(txtLocalHost, () => options.LocalHost, t => options.LocalHost = t);

            ClientOptionsControlUtils.LinkControl(lblLocalPort, txtLocalPort);
            ClientOptionsControlUtils.BindInt32(txtLocalPort, () => options.LocalPort, t => options.LocalPort = t);

            ClientOptionsControlUtils.LinkControl(lblConnectionTimeout, txtConnectionTimeout);
            ClientOptionsControlUtils.BindInt32(txtConnectionTimeout, () => options.ConnectionTimeout, t => options.ConnectionTimeout = t);
            
            ClientOptionsControlUtils.LinkControl(lblTransportTimeout, txtTransportTimeout);
            ClientOptionsControlUtils.BindInt32(txtTransportTimeout, () => options.TransportTimeout, t => options.TransportTimeout = t);
        }
    }
}
