using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Projector.Core.Subscription;

public class SubscriptionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SubscriptionWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriptions = _serviceProvider.GetServices<IProjectionSubscription>();
        var initializeTasks = subscriptions.Select(subscription => subscription.Initialize(cancellationToken));
        await Task.WhenAll(initializeTasks);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptions = _serviceProvider.GetServices<IProjectionSubscription>();
        var subscribeTasks = subscriptions.Select(subscription => subscription.Subscribe(stoppingToken));
        await Task.WhenAll(subscribeTasks);
        
        var updateProjectionsTasks = subscriptions.Select(subscription => subscription.UpdateProjections(stoppingToken));
        await Task.WhenAll(updateProjectionsTasks);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var subscriptions = _serviceProvider.GetServices<IProjectionSubscription>();
        var subscribeTasks = subscriptions.Select(subscription => subscription.Stop(cancellationToken));
        await Task.WhenAll(subscribeTasks);
    }
}