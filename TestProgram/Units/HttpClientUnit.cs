using Quick.Protocol;

namespace TestProgram.Units;

public class HttpClientUnit : AbstractClientUnit
{
    public override string Name => "HTTP client";
    protected override QpClientOptions GetClientOptions() => new Quick.Protocol.Http.Client.QpHttpClientOptions()
    {
        Url = "qp.http://127.0.0.1:3011/qp_test",
        Password = "HelloQP"
    };
}