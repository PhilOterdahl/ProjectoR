using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;

namespace ProjectoR.Core.Registration;

public class ProjectoRConfigurator
{
    public ProjectoRConfigurator(IServiceCollection services)
    {
        Services = services;
        services.AddSingleton(new ConfiguredProjectors());
    }

    public IServiceCollection Services { get; private set; }
    
    public bool ProjectionNameIsUnique(string projectionName)
    {
        var configuredProjectors = Services
            .BuildServiceProvider()
            .GetRequiredService<ConfiguredProjectors>();
        
        return configuredProjectors.ProjectionNameIsUnique(projectionName);
    }
}