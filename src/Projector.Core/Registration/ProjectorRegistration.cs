using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Subscription;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Registration;

public static class ProjectoRRegistration
{
    public static IServiceCollection AddProjectoR(
        this IServiceCollection services,
        Action<ProjectoRConfigurator> configure)
    {
        var configurator = new ProjectoRConfigurator(services);
        configure(configurator);

        return services
            .AddSingleton<EventTypeResolverProvider>()
            .AddHostedService<SubscriptionWorker>();
    }
}