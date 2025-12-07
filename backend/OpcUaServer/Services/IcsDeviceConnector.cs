using Opc.UaFx;
using Opc.UaFx.Client;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OpcUaServer.Services;

/// <summary>
/// Background service that connects to ICS machines via OPC UA
/// and publishes data to RabbitMQ for SCADA processing
/// </summary>
public class IcsDeviceConnector : BackgroundService
{
    private readonly ILogger<IcsDeviceConnector> _logger;
    private readonly string _rabbitmqHost;
    private readonly List<OpcClient> _opcClients = new();

    public IcsDeviceConnector(ILogger<IcsDeviceConnector> logger, IConfiguration configuration)
    {
        _logger = logger;
        _rabbitmqHost = configuration.GetValue<string>("RABBITMQ_HOST") ?? "rabbitmq";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for OPC UA Server to start
        await Task.Delay(5000, stoppingToken);

        try
        {
            _logger.LogInformation("Starting ICS Device Connector");

            // Connect to local OPC UA Server (simulating ICS devices)
            var client = new OpcClient("opc.tcp://localhost:4840");
            client.Connect();
            _opcClients.Add(client);

            _logger.LogInformation("Connected to OPC UA Server");

            // Subscribe to data changes
            var subscription = client.SubscribeDataChange(
                new[] {
                    "ns=2;s=ICS/Machine01/Temperature",
                    "ns=2;s=ICS/Machine01/Pressure",
                    "ns=2;s=ICS/Machine01/Speed",
                    "ns=2;s=ICS/Machine01/Status",
                    "ns=2;s=ICS/Machine02/Vibration",
                    "ns=2;s=ICS/Machine02/Current",
                    "ns=2;s=ICS/PLC01/Counter",
                    "ns=2;s=ICS/PLC01/AlarmStatus"
                },
                HandleDataChange);

            _logger.LogInformation("Subscribed to {Count} OPC UA nodes", 8);

            // Keep running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ICS Device Connector");
        }
    }

    private void HandleDataChange(object? sender, OpcDataChangeReceivedEventArgs e)
    {
        try
        {
            var nodeId = e.MonitoredItem.NodeId.ToString();
            var value = e.Item.Value;
            var timestamp = e.Item.SourceTimestamp;

            _logger.LogDebug("Data change: {NodeId} = {Value} at {Timestamp}", 
                nodeId, value, timestamp);

            // Convert OPC UA node ID to SCADA tag name
            var tagName = ConvertNodeIdToTagName(nodeId);

            // Publish to RabbitMQ
            PublishToRabbitMQ(tagName, value, timestamp, "Good");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling data change");
        }
    }

    private string ConvertNodeIdToTagName(string nodeId)
    {
        // Convert: ns=2;s=ICS/Machine01/Temperature
        // To:      ICS.Machine01.Temperature
        
        var parts = nodeId.Split(';');
        if (parts.Length >= 2)
        {
            var identifier = parts[1].Substring(2); // Remove "s="
            return identifier.Replace('/', '.');
        }

        return nodeId;
    }

    private void PublishToRabbitMQ(string tagName, object? value, DateTime timestamp, string quality)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitmqHost,
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare exchange (if not exists)
            channel.ExchangeDeclare(
                exchange: "tag-data",
                type: ExchangeType.Topic,
                durable: true
            );

            // Create message
            var message = new
            {
                tagName,
                value = Convert.ToDouble(value),
                timestamp,
                quality,
                source = "OPC-UA"
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // Publish
            channel.BasicPublish(
                exchange: "tag-data",
                routingKey: "tag.data",
                basicProperties: null,
                body: body
            );

            _logger.LogDebug("Published to RabbitMQ: {TagName} = {Value}", tagName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing to RabbitMQ");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ICS Device Connector");

        foreach (var client in _opcClients)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
