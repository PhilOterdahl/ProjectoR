using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.EventStoreDB.Data;

public class NumberOfActiveUsersProjectionConfiguration : IEntityTypeConfiguration<NumberOfActiveUsersProjection>
{
    public void Configure(EntityTypeBuilder<NumberOfActiveUsersProjection> builder)
    {
        builder
            .ToTable("NumberOfActiveUsers")
            .HasKey(projection => projection.Country);
    }
}