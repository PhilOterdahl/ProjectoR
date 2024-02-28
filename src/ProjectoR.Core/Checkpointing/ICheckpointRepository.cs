namespace ProjectoR.Core.Checkpointing;

public interface ICheckpointRepository
{ 
    Task<Checkpoint?> TryLoad(string projectionName, CancellationToken cancellationToken = default);
    Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default);
    Task MakeCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default);
}