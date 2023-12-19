using System.Threading.Channels;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Open.ChannelExtensions;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Subscription;
using Core_EventData = ProjectoR.Core.EventData;

namespace ProjectoR.EventStoreDb.Subscription;

public class EventStoreProjectionOptions
{
    public ProjectorOptions ProjectorOptions { get; set; } = new();
    public int BatchSize { get; set; } = 1000;
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
}

internal class EventStoreProjectionSubscription<TProjector> : IProjectionSubscription 
{
    private readonly EventStoreProjectionOptions _options;
    private readonly EventStoreClient _eventStoreClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventStoreProjectionSubscription<TProjector>> _logger;
    private readonly Channel<ResolvedEvent> _eventChannel;
    private readonly ProjectorService<TProjector> _projectorService;

    private bool _stopping;
    private StreamSubscription _subscription;

    public EventStoreProjectionSubscription(
        EventStoreClient eventStoreClient,
        IServiceProvider serviceProvider, 
        ILogger<EventStoreProjectionSubscription<TProjector>> logger)
    {
        _eventStoreClient = eventStoreClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _projectorService = serviceProvider.GetRequiredService<ProjectorService<TProjector>>();
        _options = serviceProvider.GetRequiredKeyedService<EventStoreProjectionOptions>(_projectorService.ProjectionName);
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
        await _projectorService.Start(cancellationToken);
    }

    public async Task Subscribe(CancellationToken cancellationToken)
    {
        if (_stopping)
            return;

        await using var scope = _serviceProvider.CreateAsyncScope();
        var eventNames = _projectorService.EventNames;
        var filter = new SubscriptionFilterOptions(EventTypeFilter.Prefix(eventNames));
        var subscribePosition = _projectorService.Position;

        var position = subscribePosition is null
            ? FromAll.Start
            : FromAll.After(new Position((ulong)subscribePosition.Value, (ulong)subscribePosition.Value));

        _subscription = await _eventStoreClient.SubscribeToAllAsync(
            position,
            EventAppeared,
            false,
            (_, reason, exception) => SubscriptionDropped(reason, _projectorService.ProjectionName, exception, cancellationToken),
            filter,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateProjections(CancellationToken cancellationToken) =>
        await _eventChannel
            .Reader
            .Batch(_options.BatchSize, true)
            .WithTimeout(_options.BatchTimeout.Milliseconds)
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
        
        try
        {
            var eventData = batch.Select(resolvedEvent =>
                new Core_EventData(
                    resolvedEvent.Event.EventType,
                    resolvedEvent.Event.Data.ToArray(),
                    (long)resolvedEvent.Event.Position.CommitPosition
                )
            );
            await _projectorService.Project(eventData, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error when updating projection: {projection}",
                _projectorService.ProjectionName
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