using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Checkpointing;
using ProjectoR.EntityFrameworkCore.Checkpointing;
using ProjectoR.Examples.Common;

namespace ProjectoR.Examples.EventStore;

public abstract class ApplicationContext(DbContextOptions options) : DbContext(options), ICheckpointingContext
{
    public DbSet<UserProjection> UsersProjections { get; set; }
    public DbSet<AmountOfUserPerCityProjection> AmountOfUsersPerCityProjections { get; set; }
    public DbSet<AmountOfUsersPerCountryProjection> AmountOfUsersPerCountryProjections { get; set; }
    public DbSet<CheckpointState> Checkpoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly)
            .ApplyConfiguration(new CheckpointStateConfiguration());
    }
}