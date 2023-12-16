namespace ProjectoR.Core.Projector;

public abstract class BatchProjector<TConnection>(TConnection connection, IServiceProvider serviceProvider) : BaseProjector<TConnection>(connection, serviceProvider)
{
    public override string[] EventNames => EventTypes
        .Select(type => EventTypeResolver.GetName(type))
        .ToArray();
    
    protected abstract Task ProjectBatch(TConnection connection, IEnumerable<object> events, CancellationToken cancellationToken = default);
    
    protected override async Task Project(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken)
    {
        var lastPosition = events.Last().Position;
        await ProjectBatch(connection, events.Select(@event => @event.Event), cancellationToken).ConfigureAwait(false);
        await SaveCheckpoint(lastPosition, cancellationToken).ConfigureAwait(false);
    }
}