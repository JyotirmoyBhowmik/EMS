using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR;
using WebSocketService.Hubs;
using System.Text;
using System.Text.Json;

namespace WebSocketService.Services;

public class TagStreamingWorker : BackgroundService
{
    private readonly IHubContext<TagHub> _hubContext;
    private readonly ILogger<TagStreamingWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public TagStreamingWorker(IHubContext<TagHub> hubContext, ILogger<TagStreamingWorker> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tag Streaming Worker starting...");

        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = "guest",
            Password = "guest",
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Subscribe to tag data queue
        _channel.QueueDeclare("tag-stream", durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var tagData = JsonSerializer.Deserialize<TagDataMessage>(message);

                if (tagData != null)
                {
                    // Broadcast to all subscribers of this tag
                    await _hubContext.Clients
                        .Group($"tag:{tagData.TagName}")
                        .SendAsync("TagUpdate", new
                        {
                            tagName = tagData.TagName,
                            value = tagData.Value,
                            timestamp = tagData.Timestamp,
                            quality = tagData.Quality
                        }, stoppingToken);
                }

                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tag stream message");
                _channel.BasicNack(args.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("tag-stream", false, consumer);
        _logger.LogInformation("Started consuming tag stream messages");

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public class TagDataMessage
{
    public required string TagName { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public string Quality { get; set; } = "Good";
}
