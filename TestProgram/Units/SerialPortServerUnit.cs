using Quick.Protocol;

namespace TestProgram.Units;

public class SerialPortServerUnit : AbstractServerUnit
{
    public override string Name => "SerialPort server";

    protected override QpServerOptions GetServerOptions() => new Quick.Protocol.SerialPort.QpSerialPortServerOptions()
    {
        PortName = "COM2",
        Password = "HelloQP",
        ServerProgram = nameof(SerialPortServerUnit) + " 1.0"
    };
}