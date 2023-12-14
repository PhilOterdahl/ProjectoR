using Microsoft.EntityFrameworkCore;

namespace Projector.Examples.EventStoreDB.Data;

public class UserContext : DbContext
{
    public DbSet<UserProjection> Users { get; set; }
    
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(UserContext).Assembly);
    }
}