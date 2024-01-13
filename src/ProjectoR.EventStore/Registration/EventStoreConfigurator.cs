using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;
using ProjectoR.Core.Subscription;
using ProjectoR.EventStore.Checkpointing;
using ProjectoR.EventStore.Subscription;

namespace ProjectoR.EventStore.Registration;

public interface IEventStoreConfigurator
{
    IEventStoreConfigurator UseEventStoreCheckpointing();

    IEventStoreConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure = null) where TProjector : class;
}

internal class EventStoreConfigurator : IEventStoreConfigurator
{
    private readonly IProjectoRConfigurator _projectoRConfigurator;

    public EventStoreConfigurator(IProjectoRConfigurator projectoRConfigurator, string connectionString)
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
        var configurator = new ProjectorConfigurator<TProjector>(_projectoRConfigurator, configure);

        _projectoRConfigurator
            .Services
            .AddKeyedSingleton<IProjectionSubscription>(configurator.ProjectionName, (provider, _) => provider.GetRequiredService<EventStoreProjectionSubscription<TProjector>>())
            .AddSingleton<IProjectionSubscription>(provider => provider.GetRequiredService<EventStoreProjectionSubscription<TProjector>>())
            .AddSingleton<EventStoreProjectionSubscription<TProjector>>();

        return this;
    }
}