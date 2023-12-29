using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Projector;
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
    private readonly IServiceCollection _services;

    public EventStoreConfigurator(IServiceCollection services, string connectionString)
    {
        _services = services;
        _services.AddEventStoreClient(connectionString);
    }

    public IEventStoreConfigurator UseEventStoreCheckpointing()
    {
        _services
            .AddScoped<ICheckpointRepository, EventStoreCheckpointRepository>();
        
        return this;
    }
    
    public IEventStoreConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure = null) where TProjector : class
    {
        var options = new ProjectorOptions();
        configure?.Invoke(options);

        var projectorConfigurator = new ProjectorConfigurator(_services);

        projectorConfigurator.UseProjector<TProjector>(options);

        _services
            .AddKeyedSingleton(projectorConfigurator.ProjectionName, options)
            .AddSingleton<IProjectionSubscription, EventStoreProjectionSubscription<TProjector>>();

        return this;
    }
}