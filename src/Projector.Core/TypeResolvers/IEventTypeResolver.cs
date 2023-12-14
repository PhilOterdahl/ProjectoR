namespace Projector.Core.TypeResolvers;

public interface IEventTypeResolver
{
    Type GetType(string eventName);
    string GetName(Type eventType);
}
