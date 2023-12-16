using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.EventNameFormatters;
using ProjectoR.Core.Projector;

namespace ProjectoR.Core.TypeResolvers;

public class EventTypeResolverProvider
{
    private readonly IServiceProvider _serviceProvider;

    public EventTypeResolverProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEventTypeResolver GetEventTypeResolver(
        EventTypeResolverType type,
        EventTypeResolverCasing casing,
        Type? customEventTypeResolverType,
        Type[] eventTypes) =>
        type switch
        {
            EventTypeResolverType.Namespace => CreateNameSpaceEventTypeResolver(casing, eventTypes),
            EventTypeResolverType.Custom => GetCustomEventTypeResolver(customEventTypeResolverType)
        };

    private NameSpaceEventTypeResolver CreateNameSpaceEventTypeResolver(EventTypeResolverCasing casing, Type[] eventTypes)
    {
        var resolver = new NameSpaceEventTypeResolver(GetEventNameFormatter(casing));
        resolver.SetEventTypes(eventTypes);
        return resolver;
    }
    
    private IEventTypeResolver GetCustomEventTypeResolver(Type customResolverType) =>
        (IEventTypeResolver)_serviceProvider.GetRequiredService(customResolverType);

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