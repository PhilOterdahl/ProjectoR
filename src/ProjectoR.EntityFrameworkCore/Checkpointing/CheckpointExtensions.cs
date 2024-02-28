using ProjectoR.Core.Checkpointing;

namespace ProjectoR.EntityFrameworkCore.Checkpointing;

public static class CheckpointExtensions
{
    public static IQueryable<Checkpoint> SelectCheckpoint(this IQueryable<CheckpointState> checkpoints) =>
        checkpoints.Select(checkpoint => new Checkpoint(checkpoint));
}