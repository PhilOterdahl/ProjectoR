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
            .ValueGeneratedOnAdd();

        builder
            .Property(@event => @event.Name)
            .IsRequired();

        builder.HasIndex(@event => @event.Name);
        builder.HasIndex(@event => @event.Position);
    }
}