namespace GrpcService.DataAccess;

using GrpcService.DataAccess.Records;
using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
            : base(options)
    {
    }

    public DbSet<ProfileRecord> Profiles { get; init; }

    public DbSet<AvatarRecord> Avatars { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ProfileRecord>()
            .HasIndex(p => p.Phone).IsUnique();

        modelBuilder
            .Entity<ProfileRecord>()
            .HasMany(p => p.Avatars)
            .WithOne(a => a.Profile)
            .HasForeignKey(a => a.ProfileId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
