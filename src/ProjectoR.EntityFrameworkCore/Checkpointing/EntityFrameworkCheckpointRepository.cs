using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.Checkpointing;

namespace ProjectoR.EntityFrameworkCore.Checkpointing;

public class EntityFrameworkCheckpointRepository(ICheckpointingContext checkpointingContext) : ICheckpointRepository 
{
    public async Task<Checkpoint?> TryLoad(string projectionName, CancellationToken cancellationToken = default) =>
        await checkpointingContext
            .Checkpoints
            .Where(checkpoint => checkpoint.ProjectionName == projectionName)
            .SelectCheckpoint()
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    public async Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default) =>
        await checkpointingContext
            .Checkpoints
            .Where(checkpoint => checkpoint.ProjectionName == projectionName)
            .SelectCheckpoint()
            .SingleAsync(cancellationToken: cancellationToken);

    public async Task MakeCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default)
    {
        var checkPointExists = await checkpointingContext
            .Checkpoints
            .AnyAsync(c => c.ProjectionName == checkpoint.ProjectionName, cancellationToken: cancellationToken);
        
        if (checkPointExists)
            checkpointingContext.Checkpoints.Update(checkpoint);
        else
            checkpointingContext.Checkpoints.Add(checkpoint);
        
        await checkpointingContext.SaveChangesAsync(cancellationToken);
    }
}