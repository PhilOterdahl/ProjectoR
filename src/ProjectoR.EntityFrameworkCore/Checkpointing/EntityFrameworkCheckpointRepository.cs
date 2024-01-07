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
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    public async Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default) =>
        await checkpointingContext
            .Checkpoints
            .Where(checkpoint => checkpoint.ProjectionName == projectionName)
            .SelectCheckpoint()
            .AsNoTracking()
            .SingleAsync(cancellationToken: cancellationToken);

    public async Task MakeCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default)
    {
        var checkPointExists = await checkpointingContext
            .Checkpoints
            .AnyAsync(c => c.ProjectionName == checkpoint.ProjectionName, cancellationToken: cancellationToken);
        
        var alreadyTracked = checkpointingContext.Checkpoints.Entry(checkpoint).State != EntityState.Detached;
        
        if (checkPointExists && !alreadyTracked)
            checkpointingContext.Checkpoints.Update(checkpoint);
        else if (!alreadyTracked)
            checkpointingContext.Checkpoints.Add(checkpoint);
        
        await checkpointingContext.SaveChangesAsync(cancellationToken);
    }
}