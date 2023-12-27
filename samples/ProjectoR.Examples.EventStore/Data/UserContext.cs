using Microsoft.EntityFrameworkCore;

namespace ProjectoR.Examples.EventStore.Data;

public class UserContext : DbContext
{
    public DbSet<UserProjection> UsersProjections { get; set; }
    
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(UserContext).Assembly);
    }
}