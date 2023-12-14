using System.Text.Json;
using Projector.Core.TypeResolvers;

namespace Projector.Core.Projector;

public enum EventTypeResolverType
{
    Namespace,
    Custom
}

public class ProjectorOptions
{
    public JsonSerializerOptions SerializerOptions { get; set; } = JsonSerializerOptions.Default;
    public EventTypeResolverType EventTypeResolver { get; private set; } = EventTypeResolverType.Namespace;
    
    internal Type? CustomEventTypeResolverType { get; private set; }

    public ProjectorOptions UseNameSpaceEventTypeResolver()
    {
        EventTypeResolver = EventTypeResolverType.Namespace;
        return this;
    }
    
    public ProjectorOptions UseCustomEventTypeResolver<TCustomEventTypeResolver>() where TCustomEventTypeResolver : IEventTypeResolver
    {
        EventTypeResolver = EventTypeResolverType.Custom;
        CustomEventTypeResolverType = typeof(TCustomEventTypeResolver);
        return this;
    }
}