using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.TemplateName).IsUnique();
            entity.Property(t => t.TemplateName).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Subject).HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => l.SentAt);
            entity.Property(l => l.RecipientEmail).HasMaxLength(200).IsRequired();
        });
    }
}
