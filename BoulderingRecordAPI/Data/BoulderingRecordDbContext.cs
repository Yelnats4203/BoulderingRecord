using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public class BoulderingRecordDbContext(DbContextOptions options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Send> Sends => Set<Send>();

    public DbSet<Session> Sessions => Set<Session>();

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

        modelBuilder.Entity<Send>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.GymName).HasMaxLength(200);
            entity.Property(s => s.UploadedAt).IsRequired();
            entity.Property(s => s.VideoPath).IsRequired();
            entity.Property(s => s.Note).HasMaxLength(1000);
            entity.Property(s => s.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.UploaderId)
                .IsRequired();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Date).IsRequired();
            entity.Property(s => s.GymName).HasMaxLength(200);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .IsRequired();
            entity.OwnsMany(s => s.GradeRecords, gradeRecord =>
            {
                gradeRecord.ToTable("SessionGradeRecords");
                gradeRecord.WithOwner().HasForeignKey("SessionId");
                gradeRecord.Property(g => g.Grade).IsRequired();
                gradeRecord.Property(g => g.CompletedCount).IsRequired();
                gradeRecord.Property(g => g.UncompletedCount).IsRequired();
            });
        });
    }
}
