using Quick.Protocol;

namespace TestProgram.Units;

public class SerialPortServerUnit
{
    public static void Invoke()
    {
        var options = new Quick.Protocol.SerialPort.QpSerialPortServerOptions()
        {
            PortName = "COM2",
            Password = "HelloQP",
            ServerProgram = nameof(SerialPortServerUnit) + " 1.0"
        };
        var server = options.CreateServer();
        server.ChannelConnected += Server_ChannelConnected;
        server.ChannelDisconnected += Server_ChannelDisconnected;
        try
        {
            server.Start();
            Console.WriteLine($"SerialPort server started on {options.PortName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SerialPort server start failed." + ex.ToString());
        }
        Console.ReadLine();
        server.Stop();
    }

    private static void Server_ChannelConnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] connected.");
    }

    private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
    {
        Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}]disconnected.");
    }
}