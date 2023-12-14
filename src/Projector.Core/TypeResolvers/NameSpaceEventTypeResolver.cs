namespace Projector.Core.TypeResolvers;

internal class NameSpaceEventTypeResolver : IEventTypeResolver
{
    private Dictionary<string, Type> _eventTypes;

    public void SetEventTypes(IEnumerable<Type> eventTypes)
    {
        _eventTypes = eventTypes.ToDictionary(type => type.FullName);
    }

    public Type GetType(string eventName)
    {
        return _eventTypes.TryGetValue(eventName, out var type)
            ? type
            : throw new InvalidOperationException($"Type for event with name: {eventName} was not found");
    }

    public string GetName(Type eventType)
    {
        return eventType.FullName;
    }
}