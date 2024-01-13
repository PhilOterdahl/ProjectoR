using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Registration;

namespace ProjectoR.Core.Projector.Metrics;

internal static class MetricsRegistration
{
    public static IProjectoRConfigurator RegisterProjectorMetrics(this IProjectoRConfigurator configurator, ProjectorInfo projectorInfo)
    {
        configurator
            .Services
            .AddKeyedSingleton<ProjectorMetrics>(projectorInfo.ProjectionName)
            .AddKeyedSingleton<IProjectorMetrics>(projectorInfo.ProjectionName,
                (provider, _) => provider.GetRequiredKeyedService<ProjectorMetrics>(projectorInfo.ProjectionName));

        return configurator;
    }
}