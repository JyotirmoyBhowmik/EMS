using Microsoft.EntityFrameworkCore;
using EnergyManagement.Models;

namespace EnergyManagement.Data;

public class EnergyDbContext : DbContext
{
    public EnergyDbContext(DbContextOptions<EnergyDbContext> options) : base(options)
    {
    }

    public DbSet<EnergyConsumption> EnergyConsumptions => Set<EnergyConsumption>();
    public DbSet<EnergyTarget> EnergyTargets => Set<EnergyTarget>();
    public DbSet<LoadProfile> LoadProfiles => Set<LoadProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Energy Consumption
        modelBuilder.Entity<EnergyConsumption>(entity =>
        {
            entity.ToTable("energy_consumption");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").IsRequired();
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.EquipmentTagId).HasColumnName("equipment_tag_id");
            entity.Property(e => e.ActivePowerKw).HasColumnName("active_power_kw");
            entity.Property(e => e.ReactivePowerKvar).HasColumnName("reactive_power_kvar");
            entity.Property(e => e.ApparentPowerKva).HasColumnName("apparent_power_kva");
            entity.Property(e => e.PowerFactor).HasColumnName("power_factor");
            entity.Property(e => e.EnergyKwh).HasColumnName("energy_kwh");
            entity.Property(e => e.EnergyCost).HasColumnName("energy_cost");
            entity.Property(e => e.CarbonKgCo2).HasColumnName("carbon_kg_co2");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Indexes
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("idx_energy_timestamp");
            entity.HasIndex(e => e.SiteId).HasDatabaseName("idx_energy_site");
        });

        // Energy Targets
        modelBuilder.Entity<EnergyTarget>(entity =>
        {
            entity.ToTable("energy_targets");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.TargetType).HasColumnName("target_type").HasMaxLength(50);
            entity.Property(e => e.BaselineKwh).HasColumnName("baseline_kwh");
            entity.Property(e => e.TargetKwh).HasColumnName("target_kwh");
            entity.Property(e => e.TargetReductionPercent).HasColumnName("target_reduction_percent");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // Load Profiles
        modelBuilder.Entity<LoadProfile>(entity =>
        {
            entity.ToTable("load_profiles");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.ProfileDate).HasColumnName("profile_date").IsRequired();
            entity.Property(e => e.HourOfDay).HasColumnName("hour_of_day");
            entity.Property(e => e.PeakDemandKw).HasColumnName("peak_demand_kw");
            entity.Property(e => e.AverageDemandKw).HasColumnName("average_demand_kw");
            entity.Property(e => e.LoadFactor).HasColumnName("load_factor");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Unique constraint
            entity.HasIndex(e => new { e.SiteId, e.ProfileDate, e.HourOfDay }).IsUnique();
        });
    }
}
