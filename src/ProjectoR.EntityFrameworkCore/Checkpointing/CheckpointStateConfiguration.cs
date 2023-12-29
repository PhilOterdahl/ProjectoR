using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectoR.Core.Checkpointing;

namespace ProjectoR.EntityFrameworkCore.Checkpointing;

public class CheckpointStateConfiguration : IEntityTypeConfiguration<CheckpointState>
{
    public void Configure(EntityTypeBuilder<CheckpointState> builder)
    {
        builder
            .ToTable("Checkpoint", "ProjectoR")
            .HasKey(checkpoint => checkpoint.ProjectionName);
        
    }
}