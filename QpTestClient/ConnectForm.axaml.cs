using Avalonia.Controls;
using Avalonia.Interactivity;
using QpTestClient.Controls;
using Quick.Protocol;
using System.Linq;

namespace QpTestClient
{
    public partial class ConnectForm : Window
    {
        public TestConnectionInfo ConnectionInfo { get; private set; }
        private QpClientOptions clientOptions = null;

        public ConnectForm()
        {
            InitializeComponent();
            Loaded += ConnectForm_Loaded;
        }

        public void EditConnectionInfo(TestConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            TxtName.Text = connectionInfo.Name;
            Title = "编辑连接";
        }

        private void ConnectForm_Loaded(object sender, RoutedEventArgs e)
        {
            var items = QpClientTypeManager.Instance.GetAll();
            CbConnectType.Items.Clear();
            foreach (var info in items)
                CbConnectType.Items.Add(info);

            if (CbConnectType.Items.Count <= 0)
                return;

            if (ConnectionInfo != null)
            {
                var qpClientTypeName = ConnectionInfo.QpClientTypeName;
                var item = items.FirstOrDefault(t => t.TypeName == qpClientTypeName);
                CbConnectType.SelectedItem = item;
            }
            else
            {
                CbConnectType.SelectedIndex = 0;
            }
        }

        private void CbConnectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbConnectType.SelectedItem is not QpClientTypeInfo qpClientTypeInfo)
                return;

            if (ConnectionInfo != null && qpClientTypeInfo.TypeName == ConnectionInfo.QpClientTypeName)
            {
                clientOptions = ConnectionInfo.QpClientOptions.Clone();
            }
            else
            {
                clientOptions = qpClientTypeInfo.CreateOptionsInstanceFunc();
            }

            var control = new AotPropertyGrid();
            qpClientTypeInfo.EditOptions(control, clientOptions);
            control.GenerateControls();
            PnlClientOptions.Child = control;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CbConnectType.SelectedItem is not QpClientTypeInfo qpClientTypeInfo)
                return;

            var name = TxtName.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                // TODO: Show validation message
                TxtName.Focus();
                return;
            }

            ConnectionInfo = new TestConnectionInfo()
            {
                Name = name,
                QpClientTypeName = qpClientTypeInfo.TypeName,
                QpClientOptions = clientOptions,
                Instructions = ConnectionInfo?.Instructions
            };
            Close(ConnectionInfo);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
