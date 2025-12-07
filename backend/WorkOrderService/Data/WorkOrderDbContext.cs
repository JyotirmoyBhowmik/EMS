using Microsoft.EntityFrameworkCore;
using WorkOrderService.Models;

namespace WorkOrderService.Data;

public class WorkOrderDbContext : DbContext
{
    public WorkOrderDbContext(DbContextOptions<WorkOrderDbContext> options) : base(options)
    {
    }

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderTask> WorkOrderTasks => Set<WorkOrderTask>();
    public DbSet<WorkOrderMaterial> WorkOrderMaterials => Set<WorkOrderMaterial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Work Orders
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("work_orders");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).HasColumnName("id");
            entity.Property(w => w.WorkOrderNumber).HasColumnName("work_order_number");
            entity.Property(w => w.Title).HasColumnName("title");
            entity.Property(w => w.Priority).HasColumnName("priority");
            entity.Property(w => w.Status).HasColumnName("status");
            
            entity.HasIndex(w => w.WorkOrderNumber).IsUnique();
            entity.HasIndex(w => w.Status);
            entity.HasIndex(w => w.AssignedToUserId);
            entity.HasIndex(w => w.CreatedAt);
            
            entity.HasMany(w => w.Tasks)
                .WithOne(t => t.WorkOrder)
                .HasForeignKey(t => t.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(w => w.Materials)
                .WithOne(m => m.WorkOrder)
                .HasForeignKey(m => m.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Work Order Tasks
        modelBuilder.Entity<WorkOrderTask>(entity =>
        {
            entity.ToTable("work_order_tasks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(t => t.SequenceNumber).HasColumnName("sequence_number");
            entity.Property(t => t.TaskDescription).HasColumnName("task_description");
            entity.Property(t => t.IsCompleted).HasColumnName("is_completed");
            
            entity.HasIndex(t => new { t.WorkOrderId, t.SequenceNumber });
        });

        // Work Order Materials
        modelBuilder.Entity<WorkOrderMaterial>(entity =>
        {
            entity.ToTable("work_order_materials");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasColumnName("id");
            entity.Property(m => m.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(m => m.MaterialName).HasColumnName("material_name");
            entity.Property(m => m.PartNumber).HasColumnName("part_number");
            entity.Property(m => m.Quantity).HasColumnName("quantity");
        });
    }
}
