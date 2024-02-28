using ProjectoR.Core.EventNameFormatters;

namespace ProjectoR.Core.TypeResolvers;

internal class ClassNameEventTypeResolver : IEventTypeResolver
{
    private readonly Dictionary<string, Type> _eventTypes;
    private readonly IEventNameFormatter _formatter;

    public ClassNameEventTypeResolver(IEventNameFormatter formatter, IEnumerable<Type> eventTypes)
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
        return eventType.IsNested 
            ? _formatter.Format($"{GetName(eventType.DeclaringType)}{eventType.Name}")
            : _formatter.Format(eventType.Name);
    }
}