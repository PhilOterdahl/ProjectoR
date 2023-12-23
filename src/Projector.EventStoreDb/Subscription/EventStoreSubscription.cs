using EventStore.Client;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Subscription;

namespace ProjectoR.EventStoreDb.Subscription;

internal class EventStoreProjectionSubscription<TProjector>(
    EventStoreClient eventStoreClient,
    ProjectorService<TProjector> projectorService,
    ILogger<EventStoreProjectionSubscription<TProjector>> logger)
    : IProjectionSubscription
{
    private bool _stopping;
    private StreamSubscription _subscription;

    public async Task Initialize(CancellationToken cancellationToken)
    {
        await projectorService.Start(cancellationToken);
    }

    public async Task Subscribe(CancellationToken cancellationToken)
    {
        if (_stopping)
            return;
        
        var eventNames = projectorService.EventNames;
        var filter = new SubscriptionFilterOptions(EventTypeFilter.Prefix(eventNames));
        var subscribePosition = projectorService.Position;

        var position = subscribePosition is null
            ? FromAll.Start
            : FromAll.After(new Position((ulong)subscribePosition.Value, (ulong)subscribePosition.Value));

        _subscription = await eventStoreClient.SubscribeToAllAsync(
            position,
            EventAppeared,
            false,
            (_, reason, exception) => SubscriptionDropped(reason, projectorService.ProjectionName, exception, cancellationToken),
            filter,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateProjections(CancellationToken cancellationToken) =>
        await projectorService.UpdateProjections(cancellationToken);

    public async Task Stop(CancellationToken cancellationToken)
    {
        _stopping = true;
        await projectorService.Stop(cancellationToken);
        _subscription.Dispose();
    }

    private void SubscriptionDropped(
        SubscriptionDroppedReason droppedReason,
        string projection,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (droppedReason)
        {
            case SubscriptionDroppedReason.SubscriberError:
                logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a error in subscriber. error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                logger.LogInformation("Stopping ...");
                Stop(cancellationToken)
                    .GetAwaiter()
                    .GetResult();
                return;
            case SubscriptionDroppedReason.ServerError:
                logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a server error. error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                break;
            case SubscriptionDroppedReason.Disposed:
                logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because the subscription was disposed",
                    projection
                );
                break;
            default:
                logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a unknown error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                break;
        }
        
        if (_stopping)
            return;
            
        logger.LogInformation("Resubscribing ...");
        Subscribe(cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    private async Task EventAppeared(
        StreamSubscription subscription, 
        ResolvedEvent resolvedEvent, 
        CancellationToken cancellationToken)
    {
        if (_stopping)
            return;
        
        await projectorService.EventAppeared(new Core.EventData(resolvedEvent.Event.EventType, resolvedEvent.Event.Data.ToArray(), (long)resolvedEvent.Event.Position.CommitPosition), cancellationToken);
    }
}