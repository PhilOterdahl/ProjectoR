using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;

namespace ProjectoR.Core.Subscription;

public class CustomSubscriptionConfigurator<TSubscription, TProjector>
    where TProjector : class 
    where TSubscription : class
{
    public CustomSubscriptionConfigurator(IProjectoRConfigurator projectoRConfigurator, Action<ProjectorOptions>? configure = null)
    {
        var configurator = new ProjectorConfigurator<TProjector>(projectoRConfigurator, configure);
        var subscriptionInfo = new CustomSubscriptionInfo(configurator.ProjectionName, typeof(TSubscription));

        projectoRConfigurator
            .Services
            .AddSingleton<TSubscription>()
            .AddKeyedSingleton(configurator.ProjectionName, subscriptionInfo)
            .AddKeyedSingleton<CustomSubscriptionWrapper>(configurator.ProjectionName,
                (provider, _) => new CustomSubscriptionWrapper(subscriptionInfo, provider)
            )
            .AddKeyedSingleton<IProjectionSubscription>(
                configurator.ProjectionName,
                (provider, _) => provider.GetRequiredKeyedService<CustomSubscriptionWrapper>(configurator.ProjectionName)
            )
            .AddSingleton<IProjectionSubscription>(provider =>
                provider.GetRequiredKeyedService<CustomSubscriptionWrapper>(configurator.ProjectionName)
            )
            .AddKeyedSingleton<SubscribeMethodInvoker>(configurator.ProjectionName,
                (provider, _) => new SubscribeMethodInvoker(subscriptionInfo, provider)
            );
    }
}