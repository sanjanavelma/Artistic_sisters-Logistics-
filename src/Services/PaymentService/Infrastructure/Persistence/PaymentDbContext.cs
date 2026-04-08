using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();
    public DbSet<DispatchSaga> DispatchSagas => Set<DispatchSaga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── PaymentRecord ─────────────────────────────────────────────────────
        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasKey(p => p.Id);

            // One payment per order
            entity.HasIndex(p => p.OrderId).IsUnique();

            entity.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Status)
                .HasConversion<int>();
        });

        // ── DispatchSaga ──────────────────────────────────────────────────────
        modelBuilder.Entity<DispatchSaga>(entity =>
        {
            entity.HasKey(s => s.Id);

            // One saga per order
            entity.HasIndex(s => s.OrderId).IsUnique();

            entity.Property(s => s.State)
                .HasConversion<int>();
        });
    }
}
