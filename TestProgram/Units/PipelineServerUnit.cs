using Quick.Protocol;

namespace TestProgram.Units;

public class PipelineServerUnit : AbstractServerUnit
{
    public override string Name => "Pipeline server";
    protected override QpServerOptions GetServerOptions() => new Quick.Protocol.Pipeline.QpPipelineServerOptions()
    {
        PipeName = "Quick.Protocol",
        Password = "HelloQP",
        ServerProgram = nameof(PipelineServerUnit) + " 1.0"
    };
}