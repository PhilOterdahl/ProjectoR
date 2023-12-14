namespace Projector.Core.TypeResolvers;

public interface IEventTypeResolver
{
    void SetEventTypes(IEnumerable<Type> eventTypes);
    Type GetType(string eventName);
    string GetName(Type eventType);
}
