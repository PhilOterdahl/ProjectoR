using Projector.Checkpointing;

namespace Projector.Projection;

public abstract class BaseProjector<TConnection> : IProjector
{
    private Checkpoint? _checkpoint;
    private readonly TConnection _connection;
    private readonly ICheckpointRepository _checkpointRepository;
    
    public abstract string ProjectionName { get; }
    public abstract string[] EventTypes { get; }
    public long? Position => _checkpoint?.Position;
    
    protected BaseProjector(TConnection connection, ICheckpointRepository checkpointRepository)
    {
        _connection = connection;
        _checkpointRepository = checkpointRepository;
    }
    
    protected virtual Task Initialize(TConnection connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected async Task SaveCheckpoint(long lastEventPosition, CancellationToken cancellationToken)
    {
        if (_checkpoint is null)
        {
            var newCheckpoint = Checkpoint.CreateCheckpoint(ProjectionName, lastEventPosition);
            await _checkpointRepository.CreateCheckpoint(newCheckpoint, cancellationToken);
            _checkpoint = newCheckpoint;
        }
        else
            await _checkpointRepository.UpdateCheckpoint(_checkpoint.CheckpointMade(lastEventPosition), cancellationToken);
    }

    protected abstract Task Project(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken);
    
    public async Task Start(CancellationToken cancellationToken)
    {
        _checkpoint = await _checkpointRepository.TryLoad(ProjectionName, cancellationToken);
        await Initialize(_connection, cancellationToken);
    }

    public async Task Project(IEnumerable<EventRecord> events, CancellationToken cancellationToken) => await Project(_connection, events, cancellationToken);
}