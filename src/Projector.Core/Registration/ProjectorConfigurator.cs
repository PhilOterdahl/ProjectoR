using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Serialization;
using ProjectoR.Core.Subscription;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Registration;

public class ProjectoRConfigurator
{
    public IServiceCollection Services { get; }

    public ProjectorSerializationOptions SerializationOptions { get; private set; } = new();
    
    public ProjectoRConfigurator(IServiceCollection services)
    {
        Services = services
            .AddSingleton<EventTypeResolverProvider>()
            .AddHostedService<SubscriptionWorker>();
        services.AddSingleton(new ConfiguredProjectors());
    }
}