using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkOrderService.Data;
using WorkOrderService.Models;

namespace WorkOrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderDbContext _context;
    private readonly ILogger<WorkOrdersController> _logger;

    public WorkOrdersController(WorkOrderDbContext context, ILogger<WorkOrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all work orders with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkOrder>>> GetWorkOrders(
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var query = _context.WorkOrders
            .Include(w => w.Tasks)
            .Include(w => w.Materials)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(w => w.Priority == priority);

        if (assignedTo.HasValue)
            query = query.Where(w => w.AssignedToUserId == assignedTo);

        var workOrders = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return Ok(workOrders);
    }

    /// <summary>
    /// Get work order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrder>> GetWorkOrder(Guid id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Tasks)
            .Include(w => w.Materials)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
            return NotFound();

        return Ok(workOrder);
    }

    /// <summary>
    /// Create new work order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkOrder>> CreateWorkOrder([FromBody] WorkOrder workOrder)
    {
        workOrder.Id = Guid.NewGuid();
        workOrder.CreatedAt = DateTime.UtcNow;
        
        // Generate work order number if not provided
        if (string.IsNullOrEmpty(workOrder.WorkOrderNumber))
        {
            var count = await _context.WorkOrders.CountAsync();
            workOrder.WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D4}";
        }

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created work order: {WorkOrderNumber}", workOrder.WorkOrderNumber);

        return CreatedAtAction(nameof(GetWorkOrder), new { id = workOrder.Id }, workOrder);
    }

    /// <summary>
    /// Update work order
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkOrder>> UpdateWorkOrder(Guid id, [FromBody] WorkOrder workOrder)
    {
        var existing = await _context.WorkOrders.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.Title = workOrder.Title;
        existing.Description = workOrder.Description;
        existing.Priority = workOrder.Priority;
        existing.Status = workOrder.Status;
        existing.Type = workOrder.Type;
        existing.AssignedToUserId = workOrder.AssignedToUserId;
        existing.ScheduledStart = workOrder.ScheduledStart;
        existing.ScheduledEnd = workOrder.ScheduledEnd;
        existing.EstimatedHours = workOrder.EstimatedHours;
        existing.EstimatedCost = workOrder.EstimatedCost;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated work order: {WorkOrderNumber}", existing.WorkOrderNumber);

        return Ok(existing);
    }

    /// <summary>
    /// Assign work order to user
    /// </summary>
    [HttpPost("{id}/assign")]
    public async Task<ActionResult> AssignWorkOrder(Guid id, [FromBody] AssignRequest request)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null)
            return NotFound();

        workOrder.AssignedToUserId = request.UserId;
        workOrder.Status = "Assigned";
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned work order {WorkOrderNumber} to user {UserId}", 
            workOrder.WorkOrderNumber, request.UserId);

        return Ok(workOrder);
    }

    /// <summary>
    /// Complete work order
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteWorkOrder(Guid id, [FromBody] CompleteRequest request)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null)
            return NotFound();

        workOrder.Status = "Completed";
        workOrder.ActualEnd = DateTime.UtcNow;
        workOrder.ActualHours = request.ActualHours;
        workOrder.CompletionNotes = request.CompletionNotes;
        workOrder.SignatureData = request.SignatureData;
        workOrder.CompletedByUserId = request.CompletedByUserId;
        workOrder.ActualCost = request.ActualCost;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Completed work order: {WorkOrderNumber}", workOrder.WorkOrderNumber);

        return Ok(workOrder);
    }

    /// <summary>
    /// Create work order from alarm
    /// </summary>
    [HttpPost("from-alarm/{alarmId}")]
    public async Task<ActionResult<WorkOrder>> CreateFromAlarm(Guid alarmId)
    {
        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            Title = $"Alarm Response - {alarmId}",
            Description = $"Work order auto-created from alarm {alarmId}",
            Priority = "High",
            Status = "New",
            Type = "Corrective",
            AlarmId = alarmId,
            CreatedAt = DateTime.UtcNow
        };

        var count = await _context.WorkOrders.CountAsync();
        workOrder.WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D4}";

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created work order from alarm {AlarmId}: {WorkOrderNumber}", 
            alarmId, workOrder.WorkOrderNumber);

        return CreatedAtAction(nameof(GetWorkOrder), new { id = workOrder.Id }, workOrder);
    }

    /// <summary>
    /// Get work order statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult> GetStatistics()
    {
        var stats = new
        {
            total = await _context.WorkOrders.CountAsync(),
            new_orders = await _context.WorkOrders.CountAsync(w => w.Status == "New"),
            in_progress = await _context.WorkOrders.CountAsync(w => w.Status == "InProgress"),
            completed = await _context.WorkOrders.CountAsync(w => w.Status == "Completed"),
            overdue = await _context.WorkOrders.CountAsync(w => 
                w.Status != "Completed" && w.ScheduledEnd < DateTime.UtcNow),
            avg_completion_hours = await _context.WorkOrders
                .Where(w => w.ActualHours.HasValue)
                .AverageAsync(w => (double?)w.ActualHours) ?? 0
        };

        return Ok(stats);
    }
}

public class AssignRequest
{
    public Guid UserId { get; set; }
}

public class CompleteRequest
{
    public decimal? ActualHours { get; set; }
    public string? CompletionNotes { get; set; }
    public string? SignatureData { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public decimal? ActualCost { get; set; }
}
