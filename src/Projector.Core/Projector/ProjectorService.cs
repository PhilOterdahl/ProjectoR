using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Open.ChannelExtensions;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.Projector.Metrics;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector;

public interface IProjectorService
{
    public bool Stopped { get; }
    public long? Position { get; }
    public string[] EventNames { get; }
    public string ProjectionName { get; }
    Task Start(CancellationToken cancellationToken);
    Task Stop(CancellationToken cancellationToken);
    Task UpdateProjections(CancellationToken cancellationToken);
    Task EventAppeared(EventData eventData, CancellationToken cancellationToken);
    Task Project(IEnumerable<EventData> eventData, CancellationToken cancellationToken);
}

internal sealed class ProjectorService<TProjector> : IProjectorService, IAsyncDisposable
{
    private readonly ProjectorCheckpointCache _checkpointCache;
    private readonly ProjectorInfo _projectorInfo;
    private readonly IServiceProvider _serviceProvider;
    private readonly ProjectorWorkQueue _projectorWorkQueue;
    private readonly Channel<EventData> _eventChannel;
    private readonly Timer _timer;
    private readonly ProjectorMetrics _metrics;
    
    private int _eventsProcessedSinceCheckpoint;
    private int _totalEventsProcessed;
    private int _eventsProcessedLastMinute;
    private Type[] EventTypes => _projectorInfo.EventTypes;
    
    public long? Position => _checkpointCache.Checkpoint?.Position;
    public string[] EventNames { get; }
    public string ProjectionName { get; }
    public bool Stopped { get; private set; }
    
    public ProjectorService(IServiceProvider serviceProvider, ProjectorInfo projectorInfo)
    {
        _serviceProvider = serviceProvider;
        _projectorWorkQueue = serviceProvider.GetRequiredService<ProjectorWorkQueue>();
        _checkpointCache = serviceProvider.GetRequiredKeyedService<ProjectorCheckpointCache>(projectorInfo.ProjectionName);
        _metrics = serviceProvider.GetRequiredKeyedService<ProjectorMetrics>(projectorInfo.ProjectionName);
        _projectorInfo = projectorInfo;
        ProjectionName = projectorInfo.ProjectionName;
        var eventTypeResolver = serviceProvider.GetRequiredKeyedService<IEventTypeResolver>(ProjectionName);
        EventNames = EventTypes
            .Select(eventType => eventTypeResolver.GetName(eventType))
            .ToArray();
        _timer = new Timer(MeasureEventsProcessed, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        
        _eventChannel = Channel.CreateBounded<EventData>(
            new BoundedChannelOptions(Math.Max(_projectorInfo.Options.BatchingOptions.BatchSize*10, 100))
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
                AllowSynchronousContinuations = false
            }
        );
    }
    
    public async Task Start(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var checkpointRepository = scope
            .ServiceProvider
            .GetRequiredService<ICheckpointRepository>();
        var checkpoint = await checkpointRepository
            .TryLoad(ProjectionName, cancellationToken)
            .ConfigureAwait(false);
        
        if (checkpoint is not null)
            _checkpointCache.SetCheckpoint(checkpoint);
    }

    public async Task EventAppeared(EventData eventData, CancellationToken cancellationToken)
    { 
        if (Stopped)
            return;
        
        await _eventChannel
            .Writer
            .WriteAsync(eventData, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateProjections(CancellationToken cancellationToken)
    {
        var batchingOptions = _projectorInfo.Options.BatchingOptions;
        var shouldBatch = batchingOptions.BatchSize > 1;
        
        if (shouldBatch)
        {
            await _eventChannel
                .Reader
                .Batch(batchingOptions.BatchSize, true)
                .WithTimeout(batchingOptions.BatchTimeout.Milliseconds)
                .TaskReadAllAsync(cancellationToken, async batch =>
                {
                    if (Stopped)
                        return;
                    
                    await _projectorWorkQueue.Enqueue(ProjectionName, batch, _projectorInfo.Options.Priority, cancellationToken);
                })
                .ConfigureAwait(false);
        }
        else
        {
            await _eventChannel
                .Reader
                .TaskReadAllAsync(cancellationToken, async @event =>
                {
                    if (Stopped)
                        return;

                    await _projectorWorkQueue.Enqueue(ProjectionName, new[] { @event }, _projectorInfo.Options.Priority, cancellationToken);
                })
                .ConfigureAwait(false);
        }
    }

    public async Task Project(IEnumerable<EventData> eventData, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var eventTypeResolver = scope.ServiceProvider.GetRequiredKeyedService<IEventTypeResolver>(ProjectionName);
        var batchPreProcessor = scope.ServiceProvider.GetService<BatchPreProcessor<TProjector>>();
        var events = eventData
            .Select(@event => new { @event.Data, type = eventTypeResolver.GetType(@event.EventName), @event.Position })
            .Select(eventData => (JsonSerializer.Deserialize(eventData.Data, eventData.type), eventData.type, eventData.Position));

        object? dependency = null;
        if (batchPreProcessor is not null)
            dependency = await batchPreProcessor
                .Invoke(cancellationToken)
                .ConfigureAwait(false);

        switch (dependency)
        {
            case IAsyncDisposable asyncDisposable:
            {
                await using (asyncDisposable)
                {
                    await Project(
                            scope.ServiceProvider,
                            dependency,
                            events,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    return;
                }
            }
            case IDisposable disposable:
            {
                using (disposable)
                {
                    await Project(
                            scope.ServiceProvider,
                            dependency,
                            events,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    return;
                }
            }
            default:
                await Project(
                        scope.ServiceProvider,
                        dependency,
                        events,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                return;
        }
    }
    
    private async Task Project(
        IServiceProvider provider,
        object? dependency, 
        IEnumerable<(object, Type, long)> events,
        CancellationToken cancellationToken)
    {
        var checkpointingOptions = _projectorInfo.Options.CheckpointingOptions;
        var projectorMethodInvoker = provider.GetRequiredService<ProjectorMethodInvoker<TProjector>>();
        var batchPostProcessor = provider.GetService<BatchPostProcessor<TProjector>>();
        var checkpointRepository = provider.GetRequiredService<ICheckpointRepository>();
        var lastEventPosition = await ProjectEvents(
                dependency,
                events, 
                projectorMethodInvoker,
                checkpointingOptions, 
                checkpointRepository, 
                cancellationToken
            )
            .ConfigureAwait(false);
        
        if (checkpointingOptions.Strategy == CheckpointingStrategy.AfterBatch)
            await SaveCheckpoint(checkpointRepository, lastEventPosition, cancellationToken)
                .ConfigureAwait(false);
            
        if (batchPostProcessor is not null)
            await batchPostProcessor
                .Invoke(dependency, cancellationToken)
                .ConfigureAwait(false);
    }

    private async Task<long> ProjectEvents(
        object? dependency,
        IEnumerable<(object?, Type type, long Position)> events,
        ProjectorMethodInvoker<TProjector> projectorMethodInvoker,
        ProjectorCheckpointingOptions checkpointingOptions, 
        ICheckpointRepository checkpointRepository,
        CancellationToken cancellationToken)
    {
        long lastEventPosition = 0;
        
        foreach (var (@event, eventType, position) in events)
        {
            lastEventPosition = position;
            await projectorMethodInvoker
                .Invoke(@event, eventType, dependency, cancellationToken)
                .ConfigureAwait(false);
            
            _eventsProcessedSinceCheckpoint++;
            _totalEventsProcessed++;
            Interlocked.Increment(ref _eventsProcessedLastMinute);
            
            switch (checkpointingOptions.Strategy)
            {
                case CheckpointingStrategy.EveryEvent:
                case CheckpointingStrategy.Interval when _eventsProcessedSinceCheckpoint % checkpointingOptions.CheckPointingInterval == 0:
                    await SaveCheckpoint(checkpointRepository, position, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case CheckpointingStrategy.AfterBatch:
                    break;
            }
        }

        return lastEventPosition;
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        _timer.Change(Timeout.Infinite, 0);
        Stopped = true;
        await _eventChannel
            .CompleteAsync()
            .ConfigureAwait(false);
    }
    
    private void MeasureEventsProcessed(object? state)
    {
        _metrics.EventsPerMinute = _eventsProcessedLastMinute;
        _metrics.EventsProcessedSinceStartedRunning = _totalEventsProcessed;
        
        Interlocked.Exchange(ref _eventsProcessedLastMinute, 0);
    }

    private async Task SaveCheckpoint(
        ICheckpointRepository checkpointRepository,
        long lastEventPosition, 
        CancellationToken cancellationToken)
    {
        var newCheckpoint = Checkpoint.CreateCheckpoint(ProjectionName, lastEventPosition);
        await checkpointRepository
            .MakeCheckpoint(newCheckpoint, cancellationToken)
            .ConfigureAwait(false);
        _checkpointCache.SetCheckpoint(newCheckpoint);
        _eventsProcessedSinceCheckpoint = 0;
    }
    
    private IEventTypeResolver GetEventTypeResolver(IServiceProvider serviceProvider)
    {
        var serializationOptions = _projectorInfo.Options.SerializationOptions;
        return serviceProvider.GetRequiredKeyedService<IEventTypeResolver>(ProjectionName);
    }

    public async ValueTask DisposeAsync() => await _timer.DisposeAsync();
}