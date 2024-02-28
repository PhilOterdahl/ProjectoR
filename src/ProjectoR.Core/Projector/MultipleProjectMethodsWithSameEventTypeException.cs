namespace ProjectoR.Core.Projector;

public class MultipleProjectMethodsWithSameEventTypeException(string projectionName, Type eventType)
    : InvalidOperationException($"Projector with projection: {projectionName} has multiple project methods with eventType: {eventType.Name} as parameter");