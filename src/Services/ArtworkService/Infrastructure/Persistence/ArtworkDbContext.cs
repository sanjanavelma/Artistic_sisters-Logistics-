using ArtworkService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ArtworkService.Infrastructure.Persistence;
public class ArtworkDbContext : DbContext
{
    public ArtworkDbContext(DbContextOptions<ArtworkDbContext> options)
        : base(options) { }
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<NotifyMeSubscription> NotifyMeSubscriptions
        => Set<NotifyMeSubscription>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artwork>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.ArtworkCode).IsUnique();
            entity.Property(a => a.Title).HasMaxLength(300).IsRequired();
            entity.Property(a => a.Price).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ArtworkType).HasMaxLength(100);
            entity.Property(a => a.Medium).HasMaxLength(100);
        });
        modelBuilder.Entity<NotifyMeSubscription>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => new { n.ArtworkId, n.CustomerId })
                .IsUnique();
            entity.HasOne(n => n.Artwork).WithMany()
                .HasForeignKey(n => n.ArtworkId);
        });
    }
}
