using AlarmManagement.Models;
using Prometheus;

namespace AlarmManagement.Services;

public class AlarmProcessingWorker : BackgroundService
{
    private readonly RabbitMQAlarmService _rabbitMQ;
    private readonly EmailNotificationService _emailService;
    private readonly SMSNotificationService _smsService;
    private readonly ILogger<AlarmProcessingWorker> _logger;

    private static readonly List<Alarm> _activeAlarms = new();
    private static readonly Counter _alarmsProcessed = Metrics.CreateCounter(
        "scada_alarms_processed_total", "Total alarms processed");
    private static readonly Gauge _activeAlarmsCount = Metrics.CreateGauge(
        "scada_alarms_active", "Current active alarms");

    public AlarmProcessingWorker(
        RabbitMQAlarmService rabbitMQ,
        EmailNotificationService emailService,
        SMSNotificationService smsService,
        ILogger<AlarmProcessingWorker> logger)
    {
        _rabbitMQ = rabbitMQ;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Alarm Processing Worker starting...");
        _rabbitMQ.StartConsuming(ProcessAlarmEventAsync);
        return Task.CompletedTask;
    }

    private async Task<bool> ProcessAlarmEventAsync(AlarmEvent alarmEvent)
    {
        try
        {
            _logger.LogWarning(
                "ALARM: {TagName} = {Value} (Threshold: {Threshold}, Type: {Type}, Priority: {Priority})",
                alarmEvent.TagName, alarmEvent.Value, alarmEvent.Threshold, 
                alarmEvent.AlarmType, alarmEvent.Priority);

            var alarm = new Alarm
            {
                TagName = alarmEvent.TagName,
                AlarmType = alarmEvent.AlarmType,
                Priority = alarmEvent.Priority,
                Value = alarmEvent.Value,
                Threshold = alarmEvent.Threshold,
                Message = $"{alarmEvent.TagName} exceeded {alarmEvent.AlarmType} threshold: {alarmEvent.Value} > {alarmEvent.Threshold}",
                TriggeredAt = alarmEvent.Timestamp
            };

            _activeAlarms.Add(alarm);
            _alarmsProcessed.Inc();
            _activeAlarmsCount.Set(_activeAlarms.Count(a => !a.IsCleared));

            // Send notifications based on priority
            if (alarmEvent.Priority == "Critical")
            {
                await _emailService.SendAlarmNotificationAsync(
                    "admin@scada.local",
                    $"CRITICAL ALARM: {alarmEvent.TagName}",
                    alarm.Message
                );

                await _smsService.SendAlarmSMSAsync(
                    "+1234567890",
                    $"CRITICAL SCADA ALARM: {alarmEvent.TagName} = {alarmEvent.Value}"
                );
            }
            else if (alarmEvent.Priority == "High")
            {
                await _emailService.SendAlarmNotificationAsync(
                    "admin@scada.local",
                    $"HIGH ALARM: {alarmEvent.TagName}",
                    alarm.Message
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing alarm event");
            return false;
        }
    }

    public static List<Alarm> GetActiveAlarms() => _activeAlarms.Where(a => !a.IsCleared).ToList();
}
