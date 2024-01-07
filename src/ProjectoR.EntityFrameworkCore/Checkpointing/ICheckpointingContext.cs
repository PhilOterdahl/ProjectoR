using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Checkpointing;

namespace ProjectoR.EntityFrameworkCore.Checkpointing;

public interface ICheckpointingContext 
{
    DbSet<CheckpointState> Checkpoints { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}