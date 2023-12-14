using Microsoft.Extensions.DependencyInjection;
using Projector.Core.Projector;
using Projector.Core.TypeResolvers;

namespace Projector.Core.Registration;

public static class ProjectorRegistration
{
    public static IServiceCollection AddProjector<TProjector>(
        this IServiceCollection services, 
        Action<ProjectorOptions> configure) where TProjector : class, IProjector
    {
        var settings = new ProjectorOptions();
        configure?.Invoke(settings);
        var key = typeof(TProjector).FullName;
        
        return services
            .AddKeyedScoped<IEventTypeResolver>(key,)
            .AddKeyedSingleton(key, settings)
            .AddScoped<TProjector>();
    }

    private static IServiceCollection AddEventTypeResolver(
        this IServiceCollection services,
        string key,
        EventTypeResolverType resolverType)
    {
        
        return  resolverType switch 
        {
            EventTypeResolverType.Namespace => services.AddKeyedScoped<IEventTypeResolver>(key, )

        };

    }
}