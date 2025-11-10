using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;

namespace NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserEmail).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.HasIndex(e => e.IsSent);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
