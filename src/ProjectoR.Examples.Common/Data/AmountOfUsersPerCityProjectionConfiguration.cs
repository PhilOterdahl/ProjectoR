using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.Common.Data;

public class AmountOfUsersPerCityProjectionConfiguration : IEntityTypeConfiguration<AmountOfUserPerCityProjection>
{
    public void Configure(EntityTypeBuilder<AmountOfUserPerCityProjection> builder)
    {
        builder
            .ToTable("AmountOfUsersPerCity", "Projection")
            .HasKey(projection => projection.City);
    }
}