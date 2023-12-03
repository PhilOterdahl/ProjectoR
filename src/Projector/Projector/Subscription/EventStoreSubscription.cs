using System.Threading.Channels;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Open.ChannelExtensions;
using Projector.Projection;

namespace Projector.Subscription;

public record EventStoreProjectionOptions(int BatchSize = 1000, TimeSpan? BatchTimeout = null);

internal class EventStoreProjectionSubscription<TProjector> : IProjectionSubscription where TProjector : IProjector
{
    private readonly EventStoreProjectionOptions _options;
    private readonly EventStoreClient _eventStoreClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventStoreProjectionSubscription<TProjector>> _logger;
    private readonly Channel<ResolvedEvent> _eventChannel;

    private bool _stopping;
    private StreamSubscription _subscription;

    public EventStoreProjectionSubscription(
        EventStoreClient eventStoreClient,
        IServiceProvider serviceProvider, 
        ILogger<EventStoreProjectionSubscription<TProjector>> logger, 
        EventStoreProjectionOptions options)
    {
        _eventStoreClient = eventStoreClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
        _eventChannel = Channel.CreateBounded<ResolvedEvent>(
            new BoundedChannelOptions(_options.BatchSize*10)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
                AllowSynchronousContinuations = false
            }
        );
    }
    
    public async Task Initialize(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var projector = serviceProvider.GetRequiredService<TProjector>();
        await projector.Start(cancellationToken);
    }

    public async Task Subscribe(CancellationToken cancellationToken)
    {
        if (_stopping)
            return;

        await using var scope = _serviceProvider.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var eventTypes = projector.EventTypes;
        var filter = new SubscriptionFilterOptions(EventTypeFilter.Prefix(eventTypes));
        var subscribePosition = projector.Position;

        var position = subscribePosition is null
            ? FromAll.Start
            : FromAll.After(new Position((ulong)subscribePosition.Value, (ulong)subscribePosition.Value));

        _subscription = await _eventStoreClient.SubscribeToAllAsync(
            position,
            EventAppeared,
            false,
            (_, reason, exception) => SubscriptionDropped(reason, projector.ProjectionName, exception, cancellationToken),
            filter,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateProjections(CancellationToken cancellationToken) =>
        await _eventChannel
            .Reader
            .Batch(_options.BatchSize, true)
            .WithTimeout(_options.BatchTimeout?.Milliseconds ?? 1000)
            .TaskReadAllAsync(cancellationToken, async batch => await Project(batch, cancellationToken));

    public async Task Stop(CancellationToken cancellationToken)
    {
        _stopping = true;
        await _eventChannel.CompleteAsync();
        _subscription.Dispose();
    }

    private async Task Project(IEnumerable<ResolvedEvent> batch, CancellationToken cancellationToken)
    {
        if (_stopping)
            return;
        
        await using var scope = _serviceProvider.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var serializer = serviceProvider.GetRequiredService<EventSerializer>();

        try
        {
            var events = batch.Select(@event => new EventRecord(serializer.Deserialize(@event), (long)@event.Event.Position.CommitPosition));
            await projector.Project(events, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error when updating projection: {projection}",
                projector.ProjectionName
            );

            await Stop(cancellationToken);
            throw;
        }
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
                _logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a error in subscriber. error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                _logger.LogInformation("Stopping ...");
                Stop(cancellationToken)
                    .GetAwaiter()
                    .GetResult();
                return;
            case SubscriptionDroppedReason.ServerError:
                _logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a server error. error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                break;
            case SubscriptionDroppedReason.Disposed:
                _logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because the subscription was disposed",
                    projection
                );
                break;
            default:
                _logger.LogError(
                    exception,
                    "Projection subscription for projection: {projection} dropped because of a unknown error: {exceptionMessage}",
                    projection,
                    exception.Message
                );
                break;
        }
        
        if (_stopping)
            return;
            
        _logger.LogInformation("Resubscribing ...");
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
        
        await _eventChannel.Writer.WriteAsync(resolvedEvent, cancellationToken);
    }
}