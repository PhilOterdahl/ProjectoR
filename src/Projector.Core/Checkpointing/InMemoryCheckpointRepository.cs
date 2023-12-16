using System.Collections.Concurrent;

namespace ProjectoR.Core.Checkpointing;

public class InMemoryCheckpointRepository : ICheckpointRepository
{
    private readonly ConcurrentDictionary<string, Checkpoint> _checkpoints = new();

    public Task<Checkpoint?> TryLoad(string projectionName, CancellationToken cancellationToken = default)
    {
        var result =_checkpoints.TryGetValue(projectionName, out var checkpoint)
            ? checkpoint
            : null;
        
        return Task.FromResult(result);
    }

    public Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default) =>
        _checkpoints.TryGetValue(projectionName, out var checkpoint)
            ? Task.FromResult(checkpoint)
            : throw new InvalidOperationException($"Checkpoint not found for projection: {projectionName}");

    public Task MakeCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default)
    {
        _checkpoints.AddOrUpdate(checkpoint.ProjectionName, _ =>  checkpoint, (_, _) => checkpoint);
        return Task.CompletedTask;
    }
}