using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Registration;

public static class ProjectoRRegistration
{
    public static IServiceCollection AddProjectoR(
        this IServiceCollection services,
        Action<ProjectoRConfigurator> configure)
    {
        var configurator = new ProjectoRConfigurator(services);
        configure(configurator);

        return services;
    }
}