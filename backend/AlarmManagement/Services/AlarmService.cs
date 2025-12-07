using AlarmManagement.Data;
using AlarmManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AlarmManagement.Services;

public interface IAlarmService
{
    Task ProcessTagValueAsync(Guid tagId, double value);
    Task<AlarmEvent?> CreateAlarmEventAsync(Guid ruleId, double value);
    Task ClearAlarmEventAsync(Guid ruleId);
}

public class AlarmService : IAlarmService
{
    private readonly ILogger<AlarmService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AlarmService(ILogger<AlarmService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessTagValueAsync(Guid tagId, double value)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AlarmDbContext>();

        var rules = await context.AlarmRules
            .Where(r => r.TagId == tagId && r.IsEnabled)
            .ToListAsync();

        foreach (var rule in rules)
        {
            bool isTriggered = CheckCondition(rule.Condition, value, rule.ThresholdValue);

            if (isTriggered)
            {
                // Check if already active
                var activeAlarm = await context.AlarmEvents
                    .Where(e => e.RuleId == rule.Id && !e.IsCleared)
                    .OrderByDescending(e => e.TriggeredAt)
                    .FirstOrDefaultAsync();

                if (activeAlarm == null)
                {
                    await CreateAlarmEventAsync(rule.Id, value);
                }
            }
            else
            {
                // Check if needs clearing
                await ClearAlarmEventAsync(rule.Id);
            }
        }
    }

    private bool CheckCondition(string condition, double value, double threshold)
    {
        return condition switch
        {
            "GreaterThan" => value > threshold,
            "LessThan" => value < threshold,
            "Equal" => Math.Abs(value - threshold) < 0.001,
            "GreaterThanOrEqual" => value >= threshold,
            "LessThanOrEqual" => value <= threshold,
            _ => false
        };
    }

    public async Task<AlarmEvent?> CreateAlarmEventAsync(Guid ruleId, double value)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AlarmDbContext>();

        var rule = await context.AlarmRules.FindAsync(ruleId);
        if (rule == null) return null;

        var alarmEvent = new AlarmEvent
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            TriggerValue = value,
            TriggeredAt = DateTime.UtcNow,
            IsAcknowledged = false,
            IsCleared = false
        };

        context.AlarmEvents.Add(alarmEvent);
        await context.SaveChangesAsync();

        _logger.LogWarning("Alarm Triggered: {RuleName} at {Value}", rule.Name, value);
        
        // TODO: Send notifications (Email/SMS) via integration events or direct call
        
        return alarmEvent;
    }

    public async Task ClearAlarmEventAsync(Guid ruleId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AlarmDbContext>();

        var activeAlarms = await context.AlarmEvents
            .Where(e => e.RuleId == ruleId && !e.IsCleared)
            .ToListAsync();

        if (activeAlarms.Any())
        {
            foreach (var alarm in activeAlarms)
            {
                alarm.IsCleared = true;
                alarm.ClearedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
            _logger.LogInformation("Alarm Cleared: RuleId {RuleId}", ruleId);
        }
    }
}
