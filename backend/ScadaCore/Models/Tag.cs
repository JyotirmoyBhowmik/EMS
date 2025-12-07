namespace ScadaCore.Models;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "Float";
    public string? Unit { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public Guid? SiteId { get; set; }
    public Site? Site { get; set; }
    public string? DeviceType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Site
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string Timezone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AlarmRule
{
    public Guid Id { get; set; }
    public Guid TagId { get; set; }
    public Tag? Tag { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty; // GreaterThan, LessThan, Equal, etc.
    public double ThresholdValue { get; set; }
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
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

public class ReportTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateType { get; set; } = "PDF"; // PDF, Excel
    public string? Frequency { get; set; } // Daily, Weekly, Monthly
    public string? Recipients { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class SystemConfig
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
