namespace ScadaCore.Models;

/// <summary>
/// Represents a SCADA tag (data point) in the system
/// </summary>
public class Tag
{
    /// <summary>
    /// Unique identifier for the tag
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tag name (unique identifier for addressing)
    /// Example: "SITE01.PLC01.Temperature"
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Data type: Int, Float, Bool, String
    /// </summary>
    public required string DataType { get; set; }

    /// <summary>
    /// Engineering units (e.g., "°C", "PSI", "kW")
    /// </summary>
    public string? Units { get; set; }

    /// <summary>
    /// Minimum expected value (for validation)
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// Maximum expected value (for validation)
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// Scaling factor: ActualValue = RawValue * ScaleFactor + Offset
    /// </summary>
    public double ScaleFactor { get; set; } = 1.0;

    /// <summary>
    /// Offset for scaling calculation
    /// </summary>
    public double Offset { get; set; } = 0.0;

    /// <summary>
    /// Scan rate in milliseconds
    /// </summary>
    public int ScanRate { get; set; } = 1000;

    /// <summary>
    /// Site/location identifier
    /// </summary>
    public string? Site { get; set; }

    /// <summary>
    /// Device/equipment identifier
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// Is this tag enabled for scanning?
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Should historical data be stored?
    /// </summary>
    public bool LogHistory { get; set; } = true;

    /// <summary>
    /// Tag creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Associated alarm rules
    /// </summary>
    public ICollection<AlarmRule>? AlarmRules { get; set; }
}

/// <summary>
/// Real-time tag value (stored in InfluxDB)
/// </summary>
public class TagValue
{
    public required string TagName { get; set; }
    public required object Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Quality { get; set; } = "Good";
}

/// <summary>
/// Alarm rule associated with a tag
/// </summary>
public class AlarmRule
{
    public int Id { get; set; }
    public int TagId { get; set; }
    public Tag? Tag { get; set; }

    /// <summary>
    /// Alarm type: HighHigh, High, Low, LowLow, RateOfChange
    /// </summary>
    public required string AlarmType { get; set; }

    /// <summary>
    /// Threshold value that triggers the alarm
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Alarm priority: Critical, High, Medium, Low
    /// </summary>
    public required string Priority { get; set; }

    /// <summary>
    /// Alarm message template
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Is this alarm rule enabled?
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Deadband to prevent alarm chattering
    /// </summary>
    public double Deadband { get; set; } = 0.0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
