using Quick.Build;
using TestProgram;
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

var units = new IUnit[]
{
    new ProcessCommunicateUnit(),
    new TcpServerUnit(),
    new TcpClientUnit(),
    new PipelineServerUnit(),
    new PipelineClientUnit(),
    new SerialPortServerUnit(),
    new SerialPortClientUnit(),
    new HttpServerUnit(),
    new HttpClientUnit(),
    new WebSocketServerUnit(),
    new WebSocketClientUnit()
};

var unitDict = units.ToDictionary(t => t.Name, t => t);

Console.WriteLine("Welcome to use Test program.");
Console.WriteLine("Please select test unit:");
var selectedUnit = QbSelect.ArrowSelect(unitDict.Keys.ToDictionary(t => t, t => t).ToArray());

Console.WriteLine($"Run unit: {selectedUnit}");
var unit = unitDict[selectedUnit];
unit.Invoke();
