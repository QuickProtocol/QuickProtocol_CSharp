using Quick.Protocol;

namespace TestProgram.Units;

public class TcpClientUnit : AbstractClientUnit
{
    public override string Name => "TCP client";
    protected override QpClientOptions GetClientOptions() => new Quick.Protocol.Tcp.QpTcpClientOptions()
    {
        Host = "127.0.0.1",
        Port = 3011,
        Password = "HelloQP",
        EnableCompress = true,
        EnableEncrypt = true
    };
}