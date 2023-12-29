using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Checkpointing;
using ProjectoR.EntityFrameworkCore;
using ProjectoR.EntityFrameworkCore.Checkpointing;

namespace ProjectoR.Examples.EventStore.Data;

public class UserContext(DbContextOptions<UserContext> options) : DbContext(options), ICheckpointingContext
{
    public DbSet<UserProjection> UsersProjections { get; set; }
    public DbSet<CheckpointState> Checkpoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(UserContext).Assembly)
            .ApplyConfiguration(new CheckpointStateConfiguration());
    }

   
}