using EventStore.Client;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;

namespace ProjectoR.EventStore.Subscription;

internal sealed class EventStoreProjectionSubscription<TProjector>(
    EventStoreClient eventStoreClient,
    ProjectorService<TProjector> projectorService,
    ILogger<EventStoreProjectionSubscription<TProjector>> logger)
    : Core.Subscription.Subscription(projectorService)
    where TProjector : class
{
    private StreamSubscription? _subscription;

    public override async Task Subscribe(CancellationToken cancellationToken)
    {
        if (Stopping)
            return;
        
        var eventNames = ProjectorService.EventNames;
        var filter = new SubscriptionFilterOptions(EventTypeFilter.Prefix(eventNames));
        var subscribePosition = ProjectorService.Position;

        var position = subscribePosition is null
            ? FromAll.Start
            : FromAll.After(new Position((ulong)subscribePosition.Value, (ulong)subscribePosition.Value));

        _subscription = await eventStoreClient
            .SubscribeToAllAsync(
                position,
                EventAppeared,
                false,
                (_, reason, exception) =>
                    SubscriptionDropped(reason, ProjectorService.ProjectionName, exception, cancellationToken),
                filter,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        
        logger.LogInformation("Subscribed to all stream from position: {position} for projection {projectionName}", position, ProjectorService.ProjectionName);
    }

    public override async Task Stop(CancellationToken cancellationToken)
    {
        await base
            .Stop(cancellationToken)
            .ConfigureAwait(false);
        _subscription?.Dispose();
        _subscription = null;
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
                logger.LogWarning(
                    exception,
                    "Projection subscription for projection: {projection} dropped because the subscription was disposed",
                    projection
                );
                return;
            default:
                logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a unknown error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                break;
        }
        
        if (Stopping)
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
        if (Stopping)
            return;

        var @event = new ProjectoR.Core.EventData(
            resolvedEvent.Event.EventType,
            resolvedEvent.Event.Data.ToArray(),
            (long)resolvedEvent.Event.Position.CommitPosition
        );
        
        logger.LogTrace(
            "Event appeared for subscription for projection: {projectionName}, eventName: {eventName} position: {position}",
            ProjectorService.ProjectionName,
            @event.EventName,
            @event.Position
        );

        await ProjectorService
            .EventAppeared(
                @event,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}