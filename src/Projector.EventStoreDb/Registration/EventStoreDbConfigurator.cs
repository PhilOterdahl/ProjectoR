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

    IEventStoreDbConfigurator UseProjector<TProjector>(Action<EventStoreProjectionOptions>? configure = null) where TProjector : class, IProjector;
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
    
    public IEventStoreDbConfigurator UseProjector<TProjector>(Action<EventStoreProjectionOptions>? configure = null) where TProjector : class, IProjector
    {
        var options = new EventStoreProjectionOptions();
        configure?.Invoke(options);
        var key = typeof(TProjector).FullName;
        
        _services
            .AddKeyedSingleton(key, options)
            .AddKeyedSingleton(key, options.ProjectorOptions)
            .AddSingleton<IProjectionSubscription, EventStoreProjectionSubscription<TProjector>>()
     
            .AddScoped<TProjector>();

        return this;
    }
}