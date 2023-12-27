using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.EventNameFormatters;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Serialization;

namespace ProjectoR.Core.TypeResolvers;

public class EventTypeResolverProvider(IServiceProvider serviceProvider)
{
    public IEventTypeResolver GetEventTypeResolver(
        EventTypeResolverType type,
        EventTypeResolverCasing casing,
        Type? customEventTypeResolverType,
        Type[] eventTypes) =>
        type switch
        {
            EventTypeResolverType.Namespace => CreateNameSpaceEventTypeResolver(casing, eventTypes),
            EventTypeResolverType.ClassName => CreateClassNameEventTypeResolver(casing, eventTypes),
            EventTypeResolverType.Custom => GetCustomEventTypeResolver(customEventTypeResolverType)
        };

    private NameSpaceEventTypeResolver CreateNameSpaceEventTypeResolver(EventTypeResolverCasing casing, Type[] eventTypes)
    {
        var resolver = new NameSpaceEventTypeResolver(GetEventNameFormatter(casing));
        resolver.SetEventTypes(eventTypes);
        return resolver;
    }
    
    private ClassNameEventTypeResolver CreateClassNameEventTypeResolver(EventTypeResolverCasing casing, Type[] eventTypes)
    {
        var resolver = new ClassNameEventTypeResolver(GetEventNameFormatter(casing));
        resolver.SetEventTypes(eventTypes);
        return resolver;
    }
    
    private IEventTypeResolver GetCustomEventTypeResolver(Type customResolverType) =>
        (IEventTypeResolver)serviceProvider.GetRequiredService(customResolverType);

    private IEventNameFormatter GetEventNameFormatter(EventTypeResolverCasing casing) =>
        casing switch
        {
            EventTypeResolverCasing.CamelCase => new CamelCaseEventNameFormatter(),
            EventTypeResolverCasing.KebabCase => new KebabCaseEventNameFormatter(),
            EventTypeResolverCasing.PascalCase => new PascalCaseEventNameFormatter(),
            EventTypeResolverCasing.SnakeCase => new SnakeCaseEventNameFormatter(),
            _ => throw new ArgumentOutOfRangeException(nameof(casing), casing, null)
        };
}