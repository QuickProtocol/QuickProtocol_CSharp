using System;

namespace SerialPortClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.Protocol.Utils.LogUtils.LogConnection = true;

            var client = new Quick.Protocol.SerialPort.QpSerialPortClient(new Quick.Protocol.SerialPort.QpSerialPortClientOptions()
            {
                PortName = "COM3",
                Password = "HelloQP"
            });

            client.Disconnected += (sender, e) =>
            {
                Console.WriteLine("连接已断开");
            };
            client.ConnectAsync().ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    Console.WriteLine("连接已取消");
                    return;
                }
                if (t.IsFaulted)
                {
                    Console.WriteLine("连接出错，原因：" + t.Exception.InnerException.ToString());
                    return;
                }
                Console.WriteLine("连接成功");
            });
            Console.ReadLine();
        }
    }
}
