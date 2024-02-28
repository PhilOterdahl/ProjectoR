using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Registration;

namespace ProjectoR.Core.Projector.Batching;

internal static class BatchingRegistration
{
    public static IProjectoRConfigurator RegisterBatchPreProcessor<TProjector>(
        this IProjectoRConfigurator configurator,
        ProjectorInfo projectorInfo)
    {
        configurator
            .Services
            .AddScoped<BatchPreProcessor<TProjector>>(provider => new BatchPreProcessor<TProjector>(projectorInfo.BatchPreProcessorInfo, provider));
        return configurator;
    }
    
    public static IProjectoRConfigurator RegisterBatchPostProcessor<TProjector>(
        this IProjectoRConfigurator configurator,
        ProjectorInfo projectorInfo)
    {
        configurator
            .Services
            .AddScoped<BatchPostProcessor<TProjector>>(provider => new BatchPostProcessor<TProjector>(projectorInfo.BatchPostProcessorInfo, provider));
        return configurator;
    }
}