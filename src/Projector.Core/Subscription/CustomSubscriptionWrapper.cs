using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;

namespace ProjectoR.Core.Subscription;

internal sealed class CustomSubscriptionWrapper(CustomSubscriptionInfo subscriptionInfo, IServiceProvider serviceProvider) 
    : Subscription(serviceProvider.GetRequiredKeyedService<IProjectorService>(subscriptionInfo.ProjectionName))
{
    private readonly ILogger<CustomSubscriptionWrapper> _logger =
        serviceProvider.GetRequiredService<ILogger<CustomSubscriptionWrapper>>();
    
    public override Task Subscribe(CancellationToken cancellationToken)
    {
        Task.Run(() => SubscribeToCustomSubscription(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task SubscribeToCustomSubscription(CancellationToken cancellationToken)
    {
        try
        {
            var eventNames = ProjectorService.EventNames;
            var subscribePosition = ProjectorService.Position;
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var subscribeMethodInvoker = scope.ServiceProvider.GetRequiredKeyedService<SubscribeMethodInvoker>(subscriptionInfo.ProjectionName);
            await foreach (var @event in subscribeMethodInvoker.Invoke(subscribePosition, eventNames, cancellationToken))
            {
                _logger.LogTrace(
                    "Event appeared for custom subscription for projection: {projectionName}, eventName: {eventName} position: {position}",
                    subscriptionInfo.ProjectionName,
                    @event.EventName,
                    @event.Position
                );
                await ProjectorService.EventAppeared(@event, cancellationToken);
            }
              
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Custom subscription for projection: {projectionName} was dropped because of an error.", subscriptionInfo.ProjectionName);
        }
    }
}