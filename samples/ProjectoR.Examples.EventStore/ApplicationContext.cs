using Microsoft.EntityFrameworkCore;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.EventStore;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options), ISampleContext
{
    public DbSet<StudentProjection> Students { get; set; }
    public DbSet<AmountOfStudentsPerCityProjection> AmountOfStudentsPerCity { get; set; }
    public DbSet<AmountOfStudentsPerCountryProjection> AmountOfStudentsPerCountry { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ISampleContext).Assembly);
    }
}