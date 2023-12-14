namespace Projector.Core.Projector;

public abstract class Projector<TConnection>(TConnection connection, IServiceProvider serviceProvider) : BaseProjector<TConnection>(connection, serviceProvider)
{
    private readonly Dictionary<Type, Func<TConnection, object, CancellationToken, Task>> _handlers = new();

    public override Type[] EventTypes => _handlers
        .Keys
        .ToArray();
    
    public override string[] EventNames => _handlers
        .Keys
        .Select(type => EventTypeResolver.GetName(type))
        .ToArray();

    protected void When<TEvent>(Func<TConnection, TEvent, CancellationToken, Task> handler) 
    {
        _handlers[typeof(TEvent)] = (connection, @event, cancellationToken) => handler(connection, (TEvent)@event, cancellationToken);
    }

    protected override async Task Project(
        TConnection connection, 
        IEnumerable<EventRecord> events,
        CancellationToken cancellationToken)
    {
        foreach (var eventRecord in events)
        {
            var (@event, position) = eventRecord;
            var eventType = @event.GetType();
            var hasHandler = _handlers.TryGetValue(eventType, out var handler);
            
            if (!hasHandler)
                throw new InvalidOperationException($"No handler found for eventType: {eventType.Name}, for projector: {GetType().Name}");

            await handler!(connection, @event, cancellationToken).ConfigureAwait(false);
            await SaveCheckpoint(position, cancellationToken).ConfigureAwait(false);
        }
    }
}