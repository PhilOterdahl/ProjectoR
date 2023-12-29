using ProjectoR.Core.Checkpointing;

namespace ProjectoR.EntityFrameworkCore;

public static class CheckpointExtensions
{
    public static IQueryable<Checkpoint> SelectCheckpoint(this IQueryable<CheckpointState> checkpoints) =>
        checkpoints.Select(checkpoint => new Checkpoint(checkpoint));
}