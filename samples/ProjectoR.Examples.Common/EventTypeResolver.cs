using ProjectoR.Core.TypeResolvers;
using ProjectoR.Examples.Common.Domain.Student.Events;

namespace ProjectoR.Examples.Common;

public class EventTypeResolver : IEventTypeResolver
{
    private readonly IReadOnlyDictionary<string, Type> _eventTypes;

    public EventTypeResolver()
    {
        _eventTypes = typeof(StudentGraduated)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace == typeof(StudentGraduated).Namespace)
            .ToDictionary(GetName, type => type);
    }

    public Type GetType(string eventName) =>
        _eventTypes.TryGetValue(eventName, out var type)
            ? type
            : throw new InvalidOperationException($"Type for event with name: {eventName} was not found");

    public string GetName(Type eventType) => eventType.Name;
}