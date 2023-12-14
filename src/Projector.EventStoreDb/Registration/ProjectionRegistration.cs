using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Projector.Core.Projector;
using Projector.Core.Registration;
using Projector.Core.Subscription;
using Projector.EventStoreDb.Subscription;

namespace Projector.EventStoreDb.Registration;

public static class ProjectionRegistration
{
    public static IServiceCollection AddEventStoreDBProjection<TProjector>(
        this IServiceCollection services,
        Action<EventStoreProjectionOptions>? configure = null) where TProjector : class, IProjector
    {
        var projectionOptions = new EventStoreProjectionOptions();
        configure?.Invoke(projectionOptions);
        
        services
            .AddProjector<TProjector>(options =>
            {
                options = projectionOptions.ProjectorOptions;
            })
            .AddSingleton(projectionOptions)
            .AddSingleton<IProjectionSubscription, EventStoreProjectionSubscription<TProjector>>();
        
        services.TryAddSingleton<IHostedService, SubscriptionWorker>();

        return services;
    }
}