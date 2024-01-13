using EventStore.Client;
using Microsoft.Extensions.Logging;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Subscription;

namespace ProjectoR.EventStore.Subscription;

internal class EventStoreProjectionSubscription<TProjector>(
    EventStoreClient eventStoreClient,
    ProjectorService<TProjector> projectorService,
    ILogger<EventStoreProjectionSubscription<TProjector>> logger)
    : IProjectionSubscription
{
    private bool _stopping;
    private StreamSubscription? _subscription;
    private readonly ProjectorService<TProjector> _projectorService = projectorService;

    public async Task Initialize(CancellationToken cancellationToken) => await projectorService.Start(cancellationToken);

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

        _subscription = await eventStoreClient
            .SubscribeToAllAsync(
                position,
                EventAppeared,
                false,
                (_, reason, exception) =>
                    SubscriptionDropped(reason, projectorService.ProjectionName, exception, cancellationToken),
                filter,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        
        logger.LogInformation("Subscribed to all stream from position: {position} for projection {projectionName}", position, _projectorService.ProjectionName);
    }

    public async Task UpdateProjections(CancellationToken cancellationToken) =>
        await _projectorService
            .UpdateProjections(cancellationToken)
            .ConfigureAwait(false);

    public async Task Stop(CancellationToken cancellationToken)
    {
        _stopping = true;
        await _projectorService
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

        await _projectorService
            .EventAppeared(
                new ProjectoR.Core.EventData(
                    resolvedEvent.Event.EventType,
                    resolvedEvent.Event.Data.ToArray(),
                    (long)resolvedEvent.Event.Position.CommitPosition
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}