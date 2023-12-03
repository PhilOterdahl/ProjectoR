using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Projector.Projection;
using Projector.Subscription;

namespace Projector.Registration;

public static class ProjectorRegistration
{
    public static IServiceCollection AddProjector<TProjector>(this IServiceCollection services, Action<EventStoreProjectionOptions>? configure = null) where TProjector : class, IProjector
    {
        var projectionSettings = new EventStoreProjectionOptions();
        configure?.Invoke(projectionSettings);

        services
            .AddScoped<IProjector, TProjector>()
            .AddSingleton<IProjectionSubscription, EventStoreProjectionSubscription<TProjector>>();
        
        services.TryAddSingleton<IHostedService, SubscriptionWorker>();
    }
}