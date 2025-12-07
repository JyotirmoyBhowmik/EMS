namespace AlarmManagement.Models;

public class Alarm
{
    public int Id { get; set; }
    public required string TagName { get; set; }
    public required string AlarmType { get; set; } // High, Low, Critical
    public required string Priority { get; set; } // Critical, High, Medium, Low
    public double Value { get; set; }
    public double Threshold { get; set; }
    public required string Message { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public bool IsAcknowledged { get; set; } = false;
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public bool IsCleared { get; set; } = false;
    public DateTime? ClearedAt { get; set; }
    public string? Site { get; set; }
    public string? Device { get; set; }
}

public class AlarmEvent
{
    public required string TagName { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public required string AlarmType { get; set; }
    public double Threshold { get; set; }
    public required string Priority { get; set; }
}
