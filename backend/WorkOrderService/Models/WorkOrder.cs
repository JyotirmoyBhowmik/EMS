using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOrderService.Models;

public class WorkOrder
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string WorkOrderNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    
    [MaxLength(50)]
    public string Status { get; set; } = "New"; // New, Assigned, InProgress, OnHold, Completed, Cancelled
    
    [MaxLength(50)]
    public string? Type { get; set; } // Corrective, Preventive, Predictive, Inspection, Calibration
    
    public Guid? AssignedToUserId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? EquipmentTagId { get; set; }
    public Guid? AlarmId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    
    [Column(TypeName = "decimal(8,2)")]
    public decimal? EstimatedHours { get; set; }
    
    [Column(TypeName = "decimal(8,2)")]
    public decimal? ActualHours { get; set; }
    
    public string? CompletionNotes { get; set; }
    public string? SignatureData { get; set; }
    public Guid? CompletedByUserId { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedCost { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? ActualCost { get; set; }
    
    // Navigation properties
    public List<WorkOrderTask> Tasks { get; set; } = new();
    public List<WorkOrderMaterial> Materials { get; set; } = new();
}

public class WorkOrderTask
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid WorkOrderId { get; set; }
    
    public int SequenceNumber { get; set; }
    
    [Required]
    public string TaskDescription { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;
}

public class WorkOrderMaterial
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid WorkOrderId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string MaterialName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? PartNumber { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal Quantity { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? UnitCost { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? TotalCost { get; set; }
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public Guid? AddedByUserId { get; set; }
    
    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;
}
