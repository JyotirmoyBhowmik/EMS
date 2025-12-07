using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DataAcquisition.Services;

/// <summary>
/// RabbitMQ service for receiving tag data from protocol drivers (Node-RED)
/// Handles high-throughput message consumption with prefetch optimization
/// </summary>
public class RabbitMQService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;

    public const string EXCHANGE_NAME = "scada.data";
    public const string QUEUE_NAME = "data-acquisition";
    public const string DEAD_LETTER_QUEUE = "data-acquisition-dlq";

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
            VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: EXCHANGE_NAME,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Declare dead-letter queue
        _channel.QueueDeclare(
            queue: DEAD_LETTER_QUEUE,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Declare main queue with DLX
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", DEAD_LETTER_QUEUE }
        };

        _channel.QueueDeclare(
            queue: QUEUE_NAME,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        // Bind queue to exchange (receive all tag data)
        _channel.QueueBind(
            queue: QUEUE_NAME,
            exchange: EXCHANGE_NAME,
            routingKey: "tag.#"); // Match all tag.* routing keys

        // Set prefetch to optimize throughput
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1000, global: false);

        _logger.LogInformation("RabbitMQ connection established");
    }

    /// <summary>
    /// Start consuming messages with a callback
    /// </summary>
    public void StartConsuming(Func<TagDataMessage, Task<bool>> messageHandler)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var tagData = JsonSerializer.Deserialize<TagDataMessage>(message);

                if (tagData != null)
                {
                    var success = await messageHandler(tagData);
                    
                    if (success)
                    {
                        _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        // Requeue failed messages
                        _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: QUEUE_NAME,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming messages from RabbitMQ");
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}

/// <summary>
/// Message format for tag data from protocol drivers
/// </summary>
public class TagDataMessage
{
    public required string TagName { get; set; }
    public required object Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Quality { get; set; } = "Good";
    public string? Site { get; set; }
    public string? Device { get; set; }
}
