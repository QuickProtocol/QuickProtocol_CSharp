using System;
using System.Windows.Forms;

namespace QpTestClient
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Quick.Protocol.QpAllClients.RegisterUriSchema();
            Quick.Protocol.SerialPort.QpSerialPortClientOptions.RegisterUriSchema();

            QpClientTypeManager.Instance.Init();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
