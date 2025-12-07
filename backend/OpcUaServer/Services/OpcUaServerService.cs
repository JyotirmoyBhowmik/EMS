using Opc.UaFx;
using Opc.UaFx.Server;

namespace OpcUaServer.Services;

public class OpcUaServerService : BackgroundService
{
    private readonly ILogger<OpcUaServerService> _logger;
    private OpcServer? _server;

    public OpcUaServerService(ILogger<OpcUaServerService> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Create OPC UA Server
            _server = new OpcServer("opc.tcp://localhost:4840", new SampleNodeManager());

            _logger.LogInformation("Starting OPC UA Server on opc.tcp://localhost:4840");
            
            _server.Start();
            
            _logger.LogInformation("OPC UA Server started successfully");
            _logger.LogInformation("Server can be accessed at: opc.tcp://localhost:4840");
            _logger.LogInformation("Available nodes:");
            _logger.LogInformation("  - ns=2;s=ICS/Machine01/Temperature");
            _logger.LogInformation("  - ns=2;s=ICS/Machine01/Pressure");
            _logger.LogInformation("  - ns=2;s=ICS/Machine01/Speed");
            _logger.LogInformation("  - ns=2;s=ICS/Machine01/Status");
            _logger.LogInformation("  - ns=2;s=ICS/Machine02/Vibration");
            _logger.LogInformation("  - ns=2;s=ICS/Machine02/Current");
            _logger.LogInformation("  - ns=2;s=ICS/PLC01/Counter");
            _logger.LogInformation("  - ns=2;s=ICS/PLC01/AlarmStatus");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start OPC UA Server");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping OPC UA Server");
        
        if (_server != null)
        {
            _server.Stop();
            _server.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Node Manager that creates simulated ICS machine data nodes
/// </summary>
public class SampleNodeManager : OpcNodeManager
{
    private readonly Random _random = new Random();
    private readonly Dictionary<string, OpcDataVariableNode> _nodes = new();

    public SampleNodeManager() : base("http://scada.company.com/ICS")
    {
    }

    protected override IEnumerable<IOpcNode> CreateNodes(OpcNodeReferenceCollection references)
    {
        var nodes = new List<IOpcNode>();

        // Create ICS folder structure
        var icsFolder = new OpcFolderNode(this.DefaultNamespace.GetName("ICS"));
        nodes.Add(icsFolder);

        // Machine 01 (Manufacturing Equipment)
        var machine01Folder = new OpcFolderNode(icsFolder, "Machine01");
        nodes.Add(machine01Folder);

        var temperatureNode = CreateDataNode(machine01Folder, "Temperature", "degrees", 0, 150);
        var pressureNode = CreateDataNode(machine01Folder, "Pressure", "bar", 0, 10);
        var speedNode = CreateDataNode(machine01Folder, "Speed", "rpm", 0, 3000);
        var statusNode = CreateBooleanNode(machine01Folder, "Status", "Running/Stopped");

        nodes.AddRange(new[] { temperatureNode, pressureNode, speedNode, statusNode });

        // Machine 02 (Motor/Pump)
        var machine02Folder = new OpcFolderNode(icsFolder, "Machine02");
        nodes.Add(machine02Folder);

        var vibrationNode = CreateDataNode(machine02Folder, "Vibration", "mm/s", 0, 20);
        var currentNode = CreateDataNode(machine02Folder, "Current", "amps", 0, 100);

        nodes.AddRange(new[] { vibrationNode, currentNode });

        // PLC 01 (Control Logic)
        var plc01Folder = new OpcFolderNode(icsFolder, "PLC01");
        nodes.Add(plc01Folder);

        var counterNode = CreateCounterNode(plc01Folder, "Counter", "Production Count");
        var alarmNode = CreateBooleanNode(plc01Folder, "AlarmStatus", "Alarm Active");

        nodes.AddRange(new[] { counterNode, alarmNode });

        // Start simulation timers
        StartSimulation();

        return nodes;
    }

    private OpcDataVariableNode<double> CreateDataNode(OpcFolderNode parent, string name, string unit, double min, double max)
    {
        var node = new OpcDataVariableNode<double>(parent, name, min + (max - min) / 2);
        node.Description = $"{name} ({unit})";
        node.EngineeringUnit = new OpcEngineeringUnitInfo(unit);
        node.InstrumentRange = new OpcValueRange(max, min);
        
        _nodes[$"{parent.Name}/{name}"] = node;
        
        return node;
    }

    private OpcDataVariableNode<bool> CreateBooleanNode(OpcFolderNode parent, string name, string description)
    {
        var node = new OpcDataVariableNode<bool>(parent, name, false);
        node.Description = description;
        
        _nodes[$"{parent.Name}/{name}"] = node;
        
        return node;
    }

    private OpcDataVariableNode<int> CreateCounterNode(OpcFolderNode parent, string name, string description)
    {
        var node = new OpcDataVariableNode<int>(parent, name, 0);
        node.Description = description;
        
        _nodes[$"{parent.Name}/{name}"] = node;
        
        return node;
    }

    private void StartSimulation()
    {
        // Simulate changing values every 2 seconds
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);

                // Update Machine01 values
                UpdateNodeValue("Machine01/Temperature", 20 + _random.NextDouble() * 80);
                UpdateNodeValue("Machine01/Pressure", 2 + _random.NextDouble() * 5);
                UpdateNodeValue("Machine01/Speed", 500 + _random.NextDouble() * 2000);
                UpdateNodeValue("Machine01/Status", _random.NextDouble() > 0.1); // 90% running

                // Update Machine02 values
                UpdateNodeValue("Machine02/Vibration", 1 + _random.NextDouble() * 10);
                UpdateNodeValue("Machine02/Current", 10 + _random.NextDouble() * 60);

                // Update PLC01 values
                var currentCounter = (int?)_nodes["PLC01/Counter"]?.Value ?? 0;
                UpdateNodeValue("PLC01/Counter", currentCounter + _random.Next(1, 5));
                UpdateNodeValue("PLC01/AlarmStatus", _random.NextDouble() > 0.95); // 5% alarm
            }
        });
    }

    private void UpdateNodeValue(string key, object value)
    {
        if (_nodes.TryGetValue(key, out var node))
        {
            node.Value = value;
            node.ApplyChanges(this.SystemContext);
        }
    }
}
