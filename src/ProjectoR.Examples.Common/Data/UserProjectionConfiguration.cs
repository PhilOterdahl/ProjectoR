using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.Common.Data;

public class UserProjectionConfiguration : IEntityTypeConfiguration<UserProjection>
{
    public void Configure(EntityTypeBuilder<UserProjection> builder)
    {
        builder
            .ToTable("User", "Projection")
            .HasKey(user => user.Id);
        
        builder.OwnsOne(user => user.ContactInformation);
        builder.OwnsOne(user => user.Address);
    }
}