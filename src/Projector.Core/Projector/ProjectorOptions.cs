using System.Text.Json;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector;

public enum EventTypeResolverType
{
    Namespace,
    Custom
}

public class ProjectorOptions
{
    public JsonSerializerOptions SerializerOptions { get; set; } = JsonSerializerOptions.Default;
    public EventTypeResolverType EventTypeResolver { get; private set; } = EventTypeResolverType.Namespace;
    public EventTypeResolverCasing Casing { get; private set; } = EventTypeResolverCasing.KebabCase;
    
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
    
    public ProjectorOptions UseSnakeCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.SnakeCase;
        return this;
    }
    
    public ProjectorOptions UsePascalCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.PascalCase;
        return this;
    }
    
    public ProjectorOptions UseCamelCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.CamelCase;
        return this;
    }
    
    public ProjectorOptions UseKebabCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.KebabCase;
        return this;
    }
}