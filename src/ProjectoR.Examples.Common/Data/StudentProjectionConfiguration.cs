using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectoR.Examples.Common.Data;

public class StudentProjectionConfiguration : IEntityTypeConfiguration<StudentProjection>
{
    public void Configure(EntityTypeBuilder<StudentProjection> builder)
    {
        builder
            .ToTable("Student", "Projection")
            .HasKey(student => student.Id);
        
        builder.OwnsOne(student => student.ContactInformation);
        builder.OwnsOne(student => student.Address);
    }
}