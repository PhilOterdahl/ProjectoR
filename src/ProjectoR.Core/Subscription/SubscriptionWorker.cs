using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ProjectoR.Core.Subscription;

public class SubscriptionWorker(IServiceProvider serviceProvider) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriptions = serviceProvider.GetServices<IProjectionSubscription>();
        var initializeTasks = subscriptions.Select(subscription => subscription.Initialize(cancellationToken));
        await Task
            .WhenAll(initializeTasks)
            .ConfigureAwait(false);

        await base.StartAsync(cancellationToken);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptions = serviceProvider.GetServices<IProjectionSubscription>();
        var subscribeTasks = subscriptions.Select(subscription => subscription.Subscribe(stoppingToken));
        await Task.WhenAll(subscribeTasks).ConfigureAwait(false);
        
        var updateProjectionsTasks = subscriptions.Select(subscription => subscription.UpdateProjections(stoppingToken));
        await Task
            .WhenAll(updateProjectionsTasks)
            .ConfigureAwait(false);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var subscriptions = serviceProvider.GetServices<IProjectionSubscription>();
        var subscribeTasks = subscriptions.Select(subscription => subscription.Stop(cancellationToken));
        await Task
            .WhenAll(subscribeTasks)
            .ConfigureAwait(false);
    }
}