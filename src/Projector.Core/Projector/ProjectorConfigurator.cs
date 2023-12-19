using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Projector;

public sealed class ProjectorConfigurator
{
    private readonly IServiceCollection _services;
    
    public string ProjectionName { get; private set; }

    public ProjectorConfigurator(IServiceCollection services)
    {
        _services = services;
    }
    
    public ProjectorConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure) where TProjector : class
    {
        var options = new ProjectorOptions();
        var projectorType = typeof(TProjector);
        var projectorInfo = new ProjectorInfo(projectorType);
        ProjectionName = projectorInfo.ProjectionName;
        configure?.Invoke(options);
        
        _services
            .AddScoped<TProjector>()
            .AddSingleton<ProjectorCheckpointCache<TProjector>>()
            .AddKeyedSingleton(projectorInfo.ProjectionName, options)
            .AddSingleton<ProjectorService<TProjector>>(serviceProvider => new ProjectorService<TProjector>(serviceProvider, projectorInfo));

        return this;
    }
}