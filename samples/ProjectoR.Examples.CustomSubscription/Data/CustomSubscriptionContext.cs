using Microsoft.EntityFrameworkCore;
using ProjectoR.Examples.EventStore;

namespace ProjectoR.Examples.CustomSubscription.Data;

public class CustomSubscriptionContext(DbContextOptions<CustomSubscriptionContext> options)
    : ApplicationContext(options)
{
    public DbSet<EventRecord> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventRecordConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}