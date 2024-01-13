using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.Projector.Metrics;
using ProjectoR.Core.Projector.Serialization;
using ProjectoR.Core.Registration;

namespace ProjectoR.Core.Projector;

internal class ConfiguredProjectors
{
    private readonly HashSet<ProjectorInfo> _projectorInformation = [];

    public int Count => _projectorInformation.Count;
    
    public bool RegisterProjector(ProjectorInfo projectorInfo) => _projectorInformation.Add(projectorInfo);

    public ProjectorInfo[] ProjectorInformation => _projectorInformation.ToArray();
}

internal sealed class ProjectorConfigurator<TProjector> where TProjector : class
{
    public string ProjectionName { get; }

    public ProjectorConfigurator(IProjectoRConfigurator projectoRConfigurator, Action<ProjectorOptions>? configure = null)
    {
        var projectorType = typeof(TProjector);
        var options = new ProjectorOptions(projectoRConfigurator.SerializationOptions.Copy());
        configure?.Invoke(options);
        var projectorInfo = new ProjectorInfo(projectorType, options);
        ProjectionName = projectorInfo.ProjectionName;

        if (!TryRegisteringProjector(projectoRConfigurator, projectorInfo))
            throw new ProjectionNameNotUniqueException(ProjectionName);

        projectoRConfigurator
            .RegisterEventNameFormatter(projectorInfo)
            .RegisterProjectorMetrics(projectorInfo)
            .RegisterCheckpointing(projectorInfo);
        
        if (projectorInfo.HasBatchPreProcessor)
            projectoRConfigurator.RegisterBatchPreProcessor<TProjector>(projectorInfo);
        
        if (projectorInfo.HasBatchPostProcessor)
            projectoRConfigurator.RegisterBatchPostProcessor<TProjector>(projectorInfo);
        
        RegisterProjectorDependencies(projectoRConfigurator, projectorInfo);
    }

    private static IProjectoRConfigurator RegisterProjectorDependencies(IProjectoRConfigurator configurator, ProjectorInfo projectorInfo)
    {
        var services = configurator.Services;

        services
            .AddKeyedSingleton(projectorInfo.ProjectionName, projectorInfo.Options)
            .AddScoped<TProjector>()
            .AddScoped<ProjectorMethodInvoker<TProjector>>(provider =>
                new ProjectorMethodInvoker<TProjector>(projectorInfo, provider)
            )
            .AddSingleton<ProjectorService<TProjector>>(serviceProvider =>
                new ProjectorService<TProjector>(serviceProvider, projectorInfo)
            )
            .AddKeyedSingleton<IProjectorService>(
                projectorInfo.ProjectionName,
                (provider, _) => provider.GetRequiredService<ProjectorService<TProjector>>()
            );
        return configurator;
    }

    private static bool TryRegisteringProjector(IProjectoRConfigurator projectoRConfigurator, ProjectorInfo projectorInfo)
    {
        var configuredProjectors = projectoRConfigurator
            .Services
            .BuildServiceProvider()
            .GetRequiredService<ConfiguredProjectors>();

        if (configuredProjectors.RegisterProjector(projectorInfo))
            return false;
        
    }
}