using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Registration;

public static class ProjectoRRegistration
{
    public static IServiceCollection AddProjectoR(
        this IServiceCollection services,
        Action<IProjectoRConfigurator> configure)
    {
        var configurator = new ProjectoRConfigurator(services);
        configure(configurator);
        configurator.Build();

        return services;
    }
}