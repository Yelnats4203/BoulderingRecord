using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public class BoulderingRecordDbContext(DbContextOptions options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Record> Records => Set<Record>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Acc).IsRequired();
            entity.HasIndex(u => u.Acc).IsUnique();
            entity.Property(u => u.Psw).IsRequired();
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.GymName).HasMaxLength(200);
            entity.Property(r => r.UploadedAt).IsRequired();
            entity.Property(r => r.VideoPath).IsRequired();
            entity.Property(r => r.Note).HasMaxLength(1000);
            entity.Property(r => r.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UploaderId)
                .IsRequired();
        });
    }
}
