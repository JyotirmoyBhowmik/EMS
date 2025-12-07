using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using AlarmManagement.Models;

namespace AlarmManagement.Services;

public class RabbitMQAlarmService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQAlarmService> _logger;

    public RabbitMQAlarmService(ILogger<RabbitMQAlarmService> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare("alarm-events", durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 100, false);
    }

    public void StartConsuming(Func<AlarmEvent, Task<bool>> handler)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var alarmEvent = JsonSerializer.Deserialize<AlarmEvent>(message);
                
                if (alarmEvent != null && await handler(alarmEvent))
                {
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                else
                {
                    _channel.BasicNack(args.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing alarm event");
                _channel.BasicNack(args.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume("alarm-events", false, consumer);
        _logger.LogInformation("Started consuming alarm events");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
