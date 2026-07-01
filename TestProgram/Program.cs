using Quick.Build;
using TestProgram.Units;

if (args != null && args.Length > 0)
{
    switch (args[0])
    {
        case nameof(ProcessCommunicateUnit.InvokeChildProcess):
            ProcessCommunicateUnit.InvokeChildProcess();
            break;
    }
}
var unitDict = new Dictionary<string, string>()
{
    [nameof(ProcessCommunicateUnit)] = "Process Communicate",
    [nameof(TcpClientUnit)] = "TCP client",
    [nameof(TcpServerUnit)] = "TCP server",
    [nameof(PipelineServerUnit)] = "Pipeline server",
    [nameof(PipelineClientUnit)] = "Pipeline client",
    [nameof(SerialPortServerUnit)] = "SerialPort server",
    [nameof(SerialPortClientUnit)] = "SerialPort client",
    [nameof(HttpServerUnit)] = "Http server",
    [nameof(HttpClientUnit)] = "Http client",
};
Console.WriteLine("Welcome to use Test program.");
Console.WriteLine("Please select test unit:");
var selectedUnit = QbSelect.ArrowSelect(unitDict.ToArray());

Console.WriteLine("Run unit: " + unitDict[selectedUnit]);
switch (selectedUnit)
{
    case nameof(ProcessCommunicateUnit):
        ProcessCommunicateUnit.Invoke();
        break;
    case nameof(TcpServerUnit):
        TcpServerUnit.Invoke();
        break;
    case nameof(TcpClientUnit):
        TcpClientUnit.Invoke();
        break;
    case nameof(PipelineServerUnit):
        PipelineServerUnit.Invoke();
        break;
    case nameof(PipelineClientUnit):
        PipelineClientUnit.Invoke();
        break;
    case nameof(SerialPortServerUnit):
        SerialPortServerUnit.Invoke();
        break;
    case nameof(SerialPortClientUnit):
        SerialPortClientUnit.Invoke();
        break;
    case nameof(HttpServerUnit):
        HttpServerUnit.Invoke();
        break;
    case nameof(HttpClientUnit):
        HttpClientUnit.Invoke();
        break;
}
