using ProjectoR.Core.EventNameFormatters;

namespace ProjectoR.Core.TypeResolvers;

internal class NameSpaceEventTypeResolver : IEventTypeResolver
{
    private Dictionary<string, Type> _eventTypes;
    private readonly IEventNameFormatter _formatter;

    public NameSpaceEventTypeResolver(IEventNameFormatter formatter, IEnumerable<Type> eventTypes)
    {
        _formatter = formatter;
        _eventTypes = eventTypes.ToDictionary(GetName);
    }

    public Type GetType(string eventName)
    {
        var name = _formatter.Format(eventName);
        return _eventTypes.TryGetValue(name, out var type)
            ? type
            : throw new InvalidOperationException($"Type for event with name: {name} was not found");
    }

    public string GetName(Type eventType)
    {
        return _formatter.Format(eventType.FullName);
    }
}