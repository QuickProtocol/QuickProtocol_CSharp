using Quick.Protocol;
using System;

namespace SerialPortServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;

            var server = new Quick.Protocol.SerialPort.QpSerialPortServer(new Quick.Protocol.SerialPort.QpSerialPortServerOptions()
            {
                PortName = "COM1",
                Password = "HelloQP",
                ServerProgram = nameof(SerialPortServer) + " 1.0"
            });
            server.ChannelConnected += Server_ChannelConnected;
            server.ChannelDisconnected += Server_ChannelDisconnected;
            try
            {
                server.Start();
                Console.WriteLine($"服务启动成功!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"服务启动失败!" + ex.ToString());
            }
            Console.ReadLine();
            server.Stop();
        }

        private static void Server_ChannelConnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已连接!");
        }

        private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: 通道[{e.ChannelName}]已断开!");
        }
    }
}
