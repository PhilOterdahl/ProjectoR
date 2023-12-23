using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Subscription;
using ProjectoR.EventStoreDB.Checkpointing;
using ProjectoR.EventStoreDb.Subscription;

namespace ProjectoR.EventStoreDb.Registration;

public interface IEventStoreDbConfigurator
{
    IEventStoreDbConfigurator UseEventStoreDBCheckpointing();

    IEventStoreDbConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure = null) where TProjector : class;
}

public class EventStoreDbConfigurator : IEventStoreDbConfigurator
{
    private readonly IServiceCollection _services;

    public EventStoreDbConfigurator(IServiceCollection services, string connectionString)
    {
        _services = services;
        _services.AddEventStoreClient(connectionString);
    }

    public IEventStoreDbConfigurator UseEventStoreDBCheckpointing()
    {
        _services
            .AddScoped<ICheckpointRepository, EventStoreDBCheckpointRepository>();
        
        return this;
    }
    
    public IEventStoreDbConfigurator UseProjector<TProjector>(Action<ProjectorOptions>? configure = null) where TProjector : class
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