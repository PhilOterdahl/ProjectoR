using Microsoft.Extensions.DependencyInjection;
using Projector.Core.Projector;

namespace Projector.Core.TypeResolvers;

public class EventTypeResolverProvider
{
    private readonly IServiceProvider _serviceProvider;

    public EventTypeResolverProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IEventTypeResolver GetEventTypeResolver(
        EventTypeResolverType type, 
        Type? customEventTypeResolverType,
        Type[] eventTypes) => 
        type switch
    {
        EventTypeResolverType.Namespace => CreateNameSpaceEventTypeResolver(eventTypes),
        EventTypeResolverType.Custom => GetCustomEventTypeResolver(customEventTypeResolverType)
    };

    private NameSpaceEventTypeResolver CreateNameSpaceEventTypeResolver(Type[] eventTypes)
    {
        var resolver = new NameSpaceEventTypeResolver();
        resolver.SetEventTypes(eventTypes);
        return resolver;
    }
    
    private IEventTypeResolver GetCustomEventTypeResolver(
        Type customResolverType) =>
        (IEventTypeResolver)_serviceProvider.GetRequiredService(customResolverType);
}