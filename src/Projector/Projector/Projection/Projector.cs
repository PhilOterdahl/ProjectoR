using Projector.Checkpointing;

namespace Projector.Projection;

public abstract class Projector<TConnection> : BaseProjector<TConnection>
{
    private readonly Dictionary<string, Func<TConnection, object, CancellationToken, Task>> _handlers = new();

    protected Projector(TConnection connection, ICheckpointRepository checkpointRepository): base(connection, checkpointRepository)
    {
    }

    protected void When<TEvent>(Func<TConnection, TEvent, CancellationToken, Task> handler) 
    {
        _handlers[typeof(TEvent).Name] = (connection, @event, cancellationToken) => handler(connection, (TEvent)@event, cancellationToken);
    }

    protected override async Task Project(
        TConnection connection, 
        IEnumerable<EventRecord> events,
        CancellationToken cancellationToken)
    {
        foreach (var eventRecord in events)
        {
            var (@event, position) = eventRecord;
            var eventType = @event.GetType().Name;
            var hasHandler = _handlers.TryGetValue(eventType, out var handler);
            
            if (!hasHandler)
                throw new InvalidOperationException($"No handler found for eventType: {eventType}, for projector: {GetType().Name}");

            await handler!(connection, @event, cancellationToken);
            await SaveCheckpoint(position, cancellationToken);
        }
    }
}