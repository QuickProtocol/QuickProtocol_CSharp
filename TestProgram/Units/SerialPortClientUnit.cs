using Quick.Protocol;

namespace TestProgram.Units;

public class SerialPortClientUnit : AbstractClientUnit
{
    public override string Name => "SerialPort client";
    protected override QpClientOptions GetClientOptions() => new Quick.Protocol.SerialPort.QpSerialPortClientOptions()
    {
        PortName = "COM3",
        Password = "HelloQP"
    };
}