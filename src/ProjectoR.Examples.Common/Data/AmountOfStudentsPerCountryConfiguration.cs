using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.Common.Data;

public class AmountOfStudentsPerCountryConfiguration  : IEntityTypeConfiguration<AmountOfStudentsPerCountryProjection>
{
    public void Configure(EntityTypeBuilder<AmountOfStudentsPerCountryProjection> builder)
    {
        builder
            .ToTable("AmountOfStudentsPerCountry", "Projection")
            .HasKey(projection => projection.CountryCode);
    }
}