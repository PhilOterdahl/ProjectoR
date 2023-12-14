using Microsoft.Extensions.DependencyInjection;
using Projector.Core.Checkpointing;

namespace Projector.Core.Projector;

public abstract class BaseProjector<TConnection> : IProjector
{
    private Checkpoint? _checkpoint;
    private readonly TConnection _connection;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly ProjectorOptions _options;

    public abstract string ProjectionName { get; }
    public abstract string[] EventNames { get; }
    public abstract Type[] EventTypes { get; }
    public long? Position => _checkpoint?.Position;
    
    protected BaseProjector(
        TConnection connection, 
        ICheckpointRepository checkpointRepository, 
        IKeyedServiceProvider serviceProvider)
    {
        _connection = connection;
        _checkpointRepository = checkpointRepository;
        _options = serviceProvider.GetRequiredKeyedService<ProjectorOptions>(GetType().FullName);
    }
    
    protected virtual Task Initialize(TConnection connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected async Task SaveCheckpoint(long lastEventPosition, CancellationToken cancellationToken)
    {
        var newCheckpoint = Checkpoint.CreateCheckpoint(ProjectionName, lastEventPosition);
        await _checkpointRepository.MakeCheckpoint(newCheckpoint, cancellationToken);
        _checkpoint = newCheckpoint;
    }

    protected abstract Task Project(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken);
    
    public async Task Start(CancellationToken cancellationToken)
    {
        _checkpoint = await _checkpointRepository.TryLoad(ProjectionName, cancellationToken);
        await Initialize(_connection, cancellationToken);
    }

    public async Task Project(IEnumerable<EventRecord> events, CancellationToken cancellationToken) => await Project(_connection, events, cancellationToken);
}