using System.Text.Json;
using EventStore.Client;

namespace Projector.Core;

public class EventSerializer
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly Dictionary<string, Type> _eventTypes;

    public EventSerializer(JsonSerializerOptions serializerOptions, Dictionary<string, Type> eventTypes)
    {
        _serializerOptions = serializerOptions;
        _eventTypes = eventTypes;
    }
    
    public object Deserialize(ResolvedEvent @event)
    {
        if (!_eventTypes.TryGetValue(@event.Event.EventType, out var eventType))
            throw new InvalidOperationException($"Type not found for eventType: {@event.Event.EventType}");
        
        return JsonSerializer.Deserialize(@event.Event.Data.ToArray(), eventType, _serializerOptions);
    }
}