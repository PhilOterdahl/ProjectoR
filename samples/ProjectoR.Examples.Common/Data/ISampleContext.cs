using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ProjectoR.Examples.Common.Data;

public interface ISampleContext
{
    public DbSet<StudentProjection> Students { get; set; }
    public DbSet<AmountOfStudentsPerCityProjection> AmountOfStudentsPerCity { get; set; }
    public DbSet<AmountOfStudentsPerCountryProjection> AmountOfStudentsPerCountry { get; set; }
    
    public DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}