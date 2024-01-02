using System.Text.Json;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector.Serialization;

public enum EventTypeResolverType
{
    Namespace,
    ClassName,
    Custom
}

public sealed class ProjectorSerializationOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = JsonSerializerOptions.Default;
    public EventTypeResolverType EventTypeResolver { get; private set; } = EventTypeResolverType.ClassName;
    public EventTypeResolverCasing Casing { get; private set; } = EventTypeResolverCasing.KebabCase;
    public Type? CustomEventTypeResolverType { get; private set; }
    
    public ProjectorSerializationOptions UseNameSpaceEventTypeResolver()
    {
        EventTypeResolver = EventTypeResolverType.Namespace;
        return this;
    }
    
    public ProjectorSerializationOptions UseClassNameEventTypeResolver()
    {
        EventTypeResolver = EventTypeResolverType.ClassName;
        return this;
    }
    
    public ProjectorSerializationOptions UseCustomEventTypeResolver<TCustomEventTypeResolver>() where TCustomEventTypeResolver : IEventTypeResolver
    {
        EventTypeResolver = EventTypeResolverType.Custom;
        CustomEventTypeResolverType = typeof(TCustomEventTypeResolver);
        return this;
    }
    
    public ProjectorSerializationOptions UseSnakeCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.SnakeCase;
        return this;
    }
    
    public ProjectorSerializationOptions UsePascalCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.PascalCase;
        return this;
    }
    
    public ProjectorSerializationOptions UseCamelCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.CamelCase;
        return this;
    }
    
    public ProjectorSerializationOptions UseKebabCaseEventNaming()
    {
        Casing = EventTypeResolverCasing.KebabCase;
        return this;
    }
}
