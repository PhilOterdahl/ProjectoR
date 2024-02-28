using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Checkpointing;
using ProjectoR.EntityFrameworkCore.Checkpointing;
using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.CustomSubscription.Data;

public class ApplicationContext(DbContextOptions<ApplicationContext> options)
    : DbContext(options), ISampleContext, ICheckpointingContext
{
    public DbSet<StudentProjection> Students { get; set; }
    public DbSet<AmountOfStudentsPerCityProjection> AmountOfStudentsPerCity { get; set; }
    public DbSet<AmountOfStudentsPerCountryProjection> AmountOfStudentsPerCountry { get; set; }
    public DbSet<CheckpointState> Checkpoints { get; set; }
    public DbSet<EventRecord> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ISampleContext).Assembly);
        modelBuilder.ApplyConfiguration(new EventRecordConfiguration());
        modelBuilder.ApplyConfiguration(new CheckpointStateConfiguration());
    }
}