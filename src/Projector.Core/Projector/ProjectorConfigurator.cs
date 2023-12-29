using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;

namespace ProjectoR.Core.Projector;

public sealed class ProjectorConfigurator
{
    private readonly IServiceCollection _services;
    
    public string ProjectionName { get; private set; }

    public ProjectorConfigurator(IServiceCollection services)
    {
        _services = services;
    }
    
    public ProjectorConfigurator UseProjector<TProjector>(ProjectorOptions options) where TProjector : class
    {
        var projectorType = typeof(TProjector);
        var projectorInfo = new ProjectorInfo(projectorType);
        ProjectionName = projectorInfo.ProjectionName;
        
        _services
            .AddKeyedSingleton<ProjectorCheckpointCache>(projectorInfo.ProjectionName)
            .AddKeyedSingleton(projectorInfo.ProjectionName, options)
            .AddScoped<TProjector>()
            .AddScoped<ProjectorMethodInvoker<TProjector>>(provider => new ProjectorMethodInvoker<TProjector>(projectorInfo, provider))
            .AddSingleton<ProjectorService<TProjector>>(serviceProvider => new ProjectorService<TProjector>(serviceProvider, projectorInfo));

        if (projectorInfo.HasBatchPreProcessor)
            _services.AddScoped<BatchPreProcessor<TProjector>>(provider => new BatchPreProcessor<TProjector>(projectorInfo.BatchPreProcessorInfo, provider));
        
        if (projectorInfo.HasBatchPostProcessor)
            _services.AddScoped<BatchPostProcessor<TProjector>>(provider => new BatchPostProcessor<TProjector>(projectorInfo.BatchPostProcessorInfo, provider));

        return this;
    }
}