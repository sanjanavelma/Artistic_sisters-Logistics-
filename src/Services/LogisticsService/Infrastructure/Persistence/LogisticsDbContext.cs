// Bridge between C# entities and SQL Server LogisticsDB
using LogisticsService.Domain.Entities;
using LogisticsService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogisticsService.Infrastructure.Persistence;

public class LogisticsDbContext : DbContext
{
    public LogisticsDbContext(DbContextOptions<LogisticsDbContext> options)
        : base(options) { }

    // These become tables in LogisticsDB
    public DbSet<DeliveryAgent> Agents => Set<DeliveryAgent>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<DeliveryAssignment> Assignments => Set<DeliveryAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── DeliveryAgent configuration ───────────────────────────────────────
        modelBuilder.Entity<DeliveryAgent>(entity =>
        {
            entity.HasKey(a => a.Id);

            // Phone must be unique — no two agents same number
            entity.HasIndex(a => a.Phone).IsUnique();

            entity.Property(a => a.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(a => a.Phone)
                .HasMaxLength(20)
                .IsRequired();

            // Store enum as int
            entity.Property(a => a.Status)
                .HasConversion<int>();
        });

        // ── Vehicle configuration ─────────────────────────────────────────────
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            // Registration must be unique
            entity.HasIndex(v => v.RegistrationNumber).IsUnique();

            entity.Property(v => v.RegistrationNumber)
                .HasMaxLength(20)
                .IsRequired();
        });

        // ── DeliveryAssignment configuration ──────────────────────────────────
        modelBuilder.Entity<DeliveryAssignment>(entity =>
        {
            entity.HasKey(a => a.Id);

            // One order can only have one assignment
            entity.HasIndex(a => a.OrderId).IsUnique();

            entity.Property(a => a.Status)
                .HasConversion<int>();

            // GPS coordinates — allow null (not always available)
            entity.Property(a => a.LastLatitude)
                .HasColumnType("decimal(10,7)");

            entity.Property(a => a.LastLongitude)
                .HasColumnType("decimal(10,7)");

            // Assignment belongs to one Agent
            entity.HasOne(a => a.Agent)
                .WithMany()
                .HasForeignKey(a => a.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment belongs to one Vehicle
            entity.HasOne(a => a.Vehicle)
                .WithMany()
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
