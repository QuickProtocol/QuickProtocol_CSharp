using Quick.Protocol;

namespace TestProgram.Units;

public class PipelineClientUnit : AbstractClientUnit
{
    public override string Name => "Pipeline client";
    protected override QpClientOptions GetClientOptions() => new Quick.Protocol.Pipeline.QpPipelineClientOptions()
    {
        PipeName = "Quick.Protocol",
        Password = "HelloQP",
        EnableCompress = true,
        EnableEncrypt = true
    };
}