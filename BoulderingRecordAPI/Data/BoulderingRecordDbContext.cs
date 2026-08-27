using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public class BoulderingRecordDbContext(DbContextOptions options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Send> Sends => Set<Send>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();

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
            entity.Property(u => u.HasEditPermission).IsRequired();
            entity.Property(u => u.IsDemoAcc).IsRequired();
        });

        modelBuilder.Entity<Send>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.GymName).HasMaxLength(200);
            entity.Property(s => s.ClimbAt).IsRequired();
            entity.Property(s => s.UploadedAt).IsRequired();
            entity.Property(s => s.VideoPublicId).IsRequired();
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
                gradeRecord.HasKey(g => g.Id);
                gradeRecord.Property(g => g.Id).ValueGeneratedNever();
                gradeRecord.Property(g => g.Grade).IsRequired();
                gradeRecord.Property(g => g.CompletedCount).IsRequired();
                gradeRecord.Property(g => g.UncompletedCount).IsRequired();
            });
        });

        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(f => f.CreatedAt).IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();
        });
    }
}
