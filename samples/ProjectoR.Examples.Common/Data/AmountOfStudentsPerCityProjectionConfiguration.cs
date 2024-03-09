using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.Common.Data;

public class AmountOfStudentsPerCityProjectionConfiguration : IEntityTypeConfiguration<AmountOfStudentsPerCityProjection>
{
    public void Configure(EntityTypeBuilder<AmountOfStudentsPerCityProjection> builder)
    {
        builder
            .ToTable("AmountOfStudentsPerCity", "Projection")
            .HasKey(projection => projection.City);
    }
}