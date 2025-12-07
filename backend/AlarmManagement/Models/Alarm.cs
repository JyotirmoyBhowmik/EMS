namespace AlarmManagement.Models;

public class AlarmRule
{
    public Guid Id { get; set; }
    public Guid TagId { get; set; }
    public Tag? Tag { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty; // GreaterThan, LessThan, Equal
    public double ThresholdValue { get; set; }
    public string Priority { get; set; } = "Medium";
    public string? Message { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class AlarmEvent
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public AlarmRule? Rule { get; set; }
    public double TriggerValue { get; set; }
    public DateTime TriggeredAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }
    public User? AcknowledgedByUser { get; set; }
    public string? AcknowledgmentComment { get; set; }
    public bool IsCleared { get; set; }
    public DateTime? ClearedAt { get; set; }
}

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
