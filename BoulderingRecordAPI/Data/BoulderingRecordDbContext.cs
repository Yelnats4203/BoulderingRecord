using BoulderingRecordAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public class BoulderingRecordDbContext(DbContextOptions<BoulderingRecordDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

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
    }
}
