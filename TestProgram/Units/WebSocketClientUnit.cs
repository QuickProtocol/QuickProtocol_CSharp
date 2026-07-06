using Quick.Protocol;

namespace TestProgram.Units;

public class WebSocketClientUnit : AbstractClientUnit
{
    public override string Name => "WebSocket client";
    protected override QpClientOptions GetClientOptions() => new Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions()
    {
        Url = "qp.ws://127.0.0.1:3011/qp_test",
        Password = "HelloQP"
    };
}