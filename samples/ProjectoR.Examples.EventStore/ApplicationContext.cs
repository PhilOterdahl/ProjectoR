using Microsoft.EntityFrameworkCore;
using ProjectoR.Examples.Common;
using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.EventStore;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options), ISampleContext
{
    public DbSet<UserProjection> UsersProjections { get; set; }
    public DbSet<AmountOfUserPerCityProjection> AmountOfUsersPerCityProjections { get; set; }
    public DbSet<AmountOfUsersPerCountryProjection> AmountOfUsersPerCountryProjections { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ISampleContext).Assembly);
    }
}