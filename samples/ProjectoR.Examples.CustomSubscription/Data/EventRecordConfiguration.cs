using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.CustomSubscription.Data;

public class EventRecordConfiguration : IEntityTypeConfiguration<EventRecord>
{
    public void Configure(EntityTypeBuilder<EventRecord> builder)
    {
        builder
            .ToTable("Event")
            .HasKey(@event => @event.Id);

        builder
            .Property(@event => @event.Position)
            .UseIdentityAlwaysColumn()
            .ValueGeneratedOnAdd();

        builder
            .Property(@event => @event.EventName)
            .IsRequired();
        
        builder
            .Property(@event => @event.StreamName)
            .IsRequired();
        
        builder
            .Property(@event => @event.Created)
            .HasDefaultValueSql("now()");
        
        builder.HasIndex(@event => @event.EventName);
        builder.HasIndex(@event => @event.StreamName);
        
        builder
            .HasIndex(@event => @event.Position)
            .IsUnique()
            .IsDescending();
    }
}