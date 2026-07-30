namespace TestProgram.Units;

public class SerialPortClientUnit
{
    public static void Invoke()
    {
        var client = new Quick.Protocol.SerialPort.QpSerialPortClient(new Quick.Protocol.SerialPort.QpSerialPortClientOptions()
            {
                PortName = "COM3",
                Password = "HelloQP"
            });

            client.Disconnected += (sender, e) =>
            {
                Console.WriteLine("Disconnected");
            };
            client.ConnectAsync().ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    Console.WriteLine("Connect cancelled.");
                    return;
                }
                if (t.IsFaulted)
                {
                    Console.WriteLine("Connect error," + t.Exception.InnerException.ToString());
                    return;
                }
                Console.WriteLine("Connected");
            });
            Console.ReadLine();
    }
}