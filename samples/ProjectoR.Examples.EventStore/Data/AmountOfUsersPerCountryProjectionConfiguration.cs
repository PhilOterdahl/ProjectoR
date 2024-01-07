using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.EventStore.Data;

public class AmountOfUsersPerCountryProjectionConfiguration  : IEntityTypeConfiguration<AmountOfUsersPerCountryProjection>
{
    public void Configure(EntityTypeBuilder<AmountOfUsersPerCountryProjection> builder)
    {
        builder
            .ToTable("AmountOfUsersPerCountry", "Projection")
            .HasKey(projection => projection.CountryCode);
    }
}