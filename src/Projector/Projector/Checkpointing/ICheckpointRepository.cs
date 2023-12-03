namespace Projector.Checkpointing;

public interface ICheckpointRepository
{ 
    Task<Checkpoint?> TryLoad(string projectionName, CancellationToken cancellationToken = default);
    Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default);
    Task CreateCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default);
    Task UpdateCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default);
}