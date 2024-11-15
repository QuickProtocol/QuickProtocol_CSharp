using Quick.Protocol.Pipeline;
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
    public partial class PipelineClientOptionsControl : UserControl
    {
        public PipelineClientOptionsControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, EventArgs e)
        {
            var options = (QpPipelineClientOptions)DataContext;

            ClientOptionsControlUtils.LinkControl(lblServerName, txtServerName);
            ClientOptionsControlUtils.BindString(txtServerName, () => options.ServerName, t => options.ServerName = t);

            ClientOptionsControlUtils.LinkControl(lblPipeName, txtPipeName);
            ClientOptionsControlUtils.BindString(txtPipeName, () => options.PipeName, t => options.PipeName = t);

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
