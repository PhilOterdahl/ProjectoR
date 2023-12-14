using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Projector.Core.Checkpointing;
using Projector.Core.TypeResolvers;

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
    private readonly EventTypeResolverProvider _eventTypeResolverProvider;

    protected IEventTypeResolver EventTypeResolver => _eventTypeResolverProvider.GetEventTypeResolver(
        _options.EventTypeResolver,
        _options.CustomEventTypeResolverType,
        EventTypes
    );
    
    protected BaseProjector(
        TConnection connection, 
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _checkpointRepository = serviceProvider.GetRequiredService<ICheckpointRepository>();
        _options = serviceProvider.GetRequiredKeyedService<ProjectorOptions>(GetType().FullName);
        _eventTypeResolverProvider = serviceProvider.GetRequiredService<EventTypeResolverProvider>();
    }
    
    protected virtual Task Initialize(TConnection connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected async Task SaveCheckpoint(long lastEventPosition, CancellationToken cancellationToken)
    {
        var newCheckpoint = Checkpoint.CreateCheckpoint(ProjectionName, lastEventPosition);
        await _checkpointRepository
            .MakeCheckpoint(newCheckpoint, cancellationToken)
            .ConfigureAwait(false);
        _checkpoint = newCheckpoint;
    }

    protected abstract Task Project(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken);
    
    public async Task Start(CancellationToken cancellationToken)
    {
        _checkpoint = await _checkpointRepository
            .TryLoad(ProjectionName, cancellationToken)
            .ConfigureAwait(false);
        await Initialize(_connection, cancellationToken).ConfigureAwait(false);
    }

    public async Task Project(IEnumerable<EventData> eventData, CancellationToken cancellationToken)
    {
        var events = eventData
            .Select(@event => new { @event.Data, type = EventTypeResolver.GetType(@event.EventName), @event.Position })
            .Select(eventData => new EventRecord(JsonSerializer.Deserialize(eventData.Data, eventData.type), eventData.Position));
        
        await Project(_connection, events, cancellationToken).ConfigureAwait(false);
    }
}