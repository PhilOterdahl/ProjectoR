using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        
        services.TryAddScoped<EventTypeResolverProvider>();
        
        return services
            .AddKeyedSingleton(key, settings)
            .AddScoped<TProjector>();
    }
}