using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.Projector.Serialization;
using ProjectoR.Core.Registration;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector;

internal class ConfiguredProjectors
{
    private readonly HashSet<string> _projectionNames = [];
    
    public bool ProjectionNameIsUnique(string projectionName) => _projectionNames.Add(projectionName);
}

internal sealed class ProjectorConfigurator<TProjector>  where TProjector : class
{
    public string ProjectionName { get; }

    public ProjectorConfigurator(ProjectoRConfigurator projectoRConfigurator, ProjectorOptions options)
    {
        var projectorType = typeof(TProjector);
        var projectorInfo = new ProjectorInfo(projectorType);
        ProjectionName = projectorInfo.ProjectionName;

        if (!projectoRConfigurator.ProjectionNameIsUnique(ProjectionName))
            throw new ProjectionNameNotUniqueException(ProjectionName);
        
        if (options.SerializationOptions.EventTypeResolver == EventTypeResolverType.Custom)
            projectoRConfigurator.Services.TryAddKeyedScoped(typeof(IEventTypeResolver), ProjectionName, options.SerializationOptions.CustomEventTypeResolverType!);

        projectoRConfigurator
            .Services
            .AddKeyedSingleton<ProjectorCheckpointCache>(projectorInfo.ProjectionName)
            .AddKeyedSingleton(projectorInfo.ProjectionName, options)
            .AddScoped<TProjector>()
            .AddScoped<ProjectorMethodInvoker<TProjector>>(provider => new ProjectorMethodInvoker<TProjector>(projectorInfo, provider))
            .AddSingleton<ProjectorService<TProjector>>(serviceProvider => new ProjectorService<TProjector>(serviceProvider, projectorInfo));

        if (projectorInfo.HasBatchPreProcessor)
            projectoRConfigurator
                .Services
                .AddScoped<BatchPreProcessor<TProjector>>(provider => new BatchPreProcessor<TProjector>(projectorInfo.BatchPreProcessorInfo, provider));
        
        if (projectorInfo.HasBatchPostProcessor)
            projectoRConfigurator
                .Services
                .AddScoped<BatchPostProcessor<TProjector>>(provider => new BatchPostProcessor<TProjector>(projectorInfo.BatchPostProcessorInfo, provider));
    }
}