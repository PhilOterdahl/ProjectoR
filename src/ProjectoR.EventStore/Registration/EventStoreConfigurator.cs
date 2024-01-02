using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.Core.Subscription;
using Projector.EventStore.Checkpointing;
using Projector.EventStore.Subscription;

namespace Projector.EventStore.Registration;

public interface IEventStoreConfigurator
{
    IEventStoreConfigurator UseEventStoreCheckpointing();

    IEventStoreConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure = null) where TProjector : class;
}

internal class EventStoreConfigurator : IEventStoreConfigurator
{
    private readonly ProjectoRConfigurator _projectoRConfigurator;

    public EventStoreConfigurator(ProjectoRConfigurator projectoRConfigurator, string connectionString)
    {
        _projectoRConfigurator = projectoRConfigurator;
        projectoRConfigurator.Services.AddEventStoreClient(connectionString);
    }

    public IEventStoreConfigurator UseEventStoreCheckpointing()
    {
        _projectoRConfigurator
            .Services
            .AddScoped<ICheckpointRepository, EventStoreCheckpointRepository>();
        
        return this;
    }
    
    public IEventStoreConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure = null) where TProjector : class
    {
        var options = new ProjectorOptions();
        configure?.Invoke(options);

        var projectorConfigurator = new ProjectorConfigurator<TProjector>(_projectoRConfigurator, options);

        _projectoRConfigurator
            .Services
            .AddKeyedSingleton(projectorConfigurator.ProjectionName, options)
            .AddSingleton<IProjectionSubscription, EventStoreProjectionSubscription<TProjector>>();

        return this;
    }
}