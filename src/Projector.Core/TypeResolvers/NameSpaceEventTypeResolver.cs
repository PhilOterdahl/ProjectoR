using ProjectoR.Core.EventNameFormatters;

namespace ProjectoR.Core.TypeResolvers;

internal class NameSpaceEventTypeResolver(IEventNameFormatter formatter) : IEventTypeResolver
{
    private Dictionary<string, Type> _eventTypes;

    public void SetEventTypes(IEnumerable<Type> eventTypes)
    {
        _eventTypes = eventTypes.ToDictionary(type => formatter.Format(type.FullName));
    }

    public Type GetType(string eventName)
    {
        var name = formatter.Format(eventName);
        return _eventTypes.TryGetValue(name, out var type)
            ? type
            : throw new InvalidOperationException($"Type for event with name: {name} was not found");
    }

    public string GetName(Type eventType)
    {
        return formatter.Format(eventType.FullName);
    }
}