using Microsoft.EntityFrameworkCore;
using AlarmManagement.Models;

namespace AlarmManagement.Data;

public class AlarmDbContext : DbContext
{
    public AlarmDbContext(DbContextOptions<AlarmDbContext> options) : base(options)
    {
    }

    public DbSet<AlarmRule> AlarmRules { get; set; } = null!;
    public DbSet<AlarmEvent> AlarmEvents { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlarmRule>(entity =>
        {
            entity.ToTable("alarm_rules");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Tag)
                .WithMany()
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AlarmEvent>(entity =>
        {
            entity.ToTable("alarm_events");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TriggeredAt);
            entity.HasIndex(e => new { e.IsAcknowledged, e.IsCleared });
            
            entity.HasOne(e => e.Rule)
                .WithMany()
                .HasForeignKey(e => e.RuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
