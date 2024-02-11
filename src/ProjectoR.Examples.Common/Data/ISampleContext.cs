using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ProjectoR.Examples.Common.Data;

public interface ISampleContext
{
    public DbSet<UserProjection> UsersProjections { get; set; }
    public DbSet<AmountOfUserPerCityProjection> AmountOfUsersPerCityProjections { get; set; }
    public DbSet<AmountOfUsersPerCountryProjection> AmountOfUsersPerCountryProjections { get; set; }
    
    public DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}