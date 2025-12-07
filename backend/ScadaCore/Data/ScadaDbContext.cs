using Microsoft.EntityFrameworkCore;
using ScadaCore.Models;

namespace ScadaCore.Data;

/// <summary>
/// Database context for SCADA configuration data
/// </summary>
public class ScadaDbContext : DbContext
{
    public ScadaDbContext(DbContextOptions<ScadaDbContext> options) : base(options)
    {
    }

    public DbSet<Tag> Tags { get; set; }
    public DbSet<AlarmRule> AlarmRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => new { e.Site, e.Device });
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Units).HasMaxLength(50);
            entity.Property(e => e.Site).HasMaxLength(100);
            entity.Property(e => e.Device).HasMaxLength(100);
        });

        // AlarmRule configuration
        modelBuilder.Entity<AlarmRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.AlarmRules)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.AlarmType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50).IsRequired();
        });

        // Seed sample data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Sample tags for testing
        modelBuilder.Entity<Tag>().HasData(
            new Tag
            {
                Id = 1,
                Name = "SITE01.TURBINE01.WindSpeed",
                Description = "Wind turbine blade speed",
                DataType = "Float",
                Units = "m/s",
                MinValue = 0,
                MaxValue = 50,
                ScanRate = 1000,
                Site = "SITE01",
                Device = "TURBINE01",
                IsEnabled = true,
                LogHistory = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = 2,
                Name = "SITE01.TURBINE01.PowerOutput",
                Description = "Turbine power generation",
                DataType = "Float",
                Units = "kW",
                MinValue = 0,
                MaxValue = 5000,
                ScanRate = 1000,
                Site = "SITE01",
                Device = "TURBINE01",
                IsEnabled = true,
                LogHistory = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = 3,
                Name = "SITE02.SOLAR01.Voltage",
                Description = "Solar panel voltage",
                DataType = "Float",
                Units = "V",
                MinValue = 0,
                MaxValue = 1000,
                ScanRate = 500,
                Site = "SITE02",
                Device = "SOLAR01",
                IsEnabled = true,
                LogHistory = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = 4,
                Name = "SITE02.SOLAR01.Current",
                Description = "Solar panel current",
                DataType = "Float",
                Units = "A",
                MinValue = 0,
                MaxValue = 100,
                ScanRate = 500,
                Site = "SITE02",
                Device = "SOLAR01",
                IsEnabled = true,
                LogHistory = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = 5,
                Name = "SITE01.BATTERY01.StateOfCharge",
                Description = "Battery state of charge percentage",
                DataType = "Float",
                Units = "%",
                MinValue = 0,
                MaxValue = 100,
                ScanRate = 2000,
                Site = "SITE01",
                Device = "BATTERY01",
                IsEnabled = true,
                LogHistory = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Sample alarm rules
        modelBuilder.Entity<AlarmRule>().HasData(
            new AlarmRule
            {
                Id = 1,
                TagId = 1,
                AlarmType = "High",
                Threshold = 40.0,
                Priority = "Medium",
                Message = "Wind speed is too high",
                Deadband = 2.0,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new AlarmRule
            {
                Id = 2,
                TagId = 2,
                AlarmType = "Low",
                Threshold = 500.0,
                Priority = "Low",
                Message = "Power output is below threshold",
                Deadband = 50.0,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new AlarmRule
            {
                Id = 3,
                TagId = 5,
                AlarmType = "LowLow",
                Threshold = 20.0,
                Priority = "Critical",
                Message = "Battery critically low!",
                Deadband = 5.0,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
