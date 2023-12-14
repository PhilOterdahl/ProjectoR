using System.Text.Json;

namespace Projector.Core.Projector;

public enum EventTypeResolverType
{
    Namespace,
    Custom
}

public class ProjectorOptions
{
    public JsonSerializerOptions SerializerOptions { get; set; } = JsonSerializerOptions.Default;
    public EventTypeResolverType EventTypeTypeResolve { get; set; } = EventTypeResolverType.Namespace;
}