using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Open.ChannelExtensions;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector;

internal sealed class ProjectorService<TProjector>
{
    private readonly ProjectorOptions _options;
    public long? Position => _checkpointCache.Checkpoint?.Position;
    private readonly ProjectorCheckpointCache _checkpointCache;
    private readonly ProjectorInfo _projectorInfo;
    private readonly IServiceProvider _serviceProvider;
    private readonly Channel<EventData> _eventChannel;
    private bool _stopping;
    private int _eventsProcessedSinceCheckpoint;
    
    public Type[] EventTypes => _projectorInfo.EventTypes;
    public string[] EventNames { get; }
    public string ProjectionName { get; }
    
    public ProjectorService(IServiceProvider serviceProvider, ProjectorInfo projectorInfo)
    {
        _serviceProvider = serviceProvider;
        _options = serviceProvider.GetRequiredKeyedService<ProjectorOptions>(projectorInfo.ProjectionName);
        _checkpointCache = serviceProvider.GetRequiredKeyedService<ProjectorCheckpointCache>(projectorInfo.ProjectionName);
        _projectorInfo = projectorInfo;
        var eventTypeResolver = GetEventTypeResolver(serviceProvider);
        EventNames = EventTypes
            .Select(eventType => eventTypeResolver.GetName(eventType))
            .ToArray();
        ProjectionName = projectorInfo.ProjectionName;
        _eventChannel = Channel.CreateBounded<EventData>(
            new BoundedChannelOptions(_options.BatchingOptions.BatchSize*10)
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
        if (_stopping)
            return;
        
        await _eventChannel
            .Writer
            .WriteAsync(eventData, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateProjections(CancellationToken cancellationToken) =>
        await _eventChannel
            .Reader
            .Batch(_options.BatchingOptions.BatchSize, true)
            .WithTimeout(_options.BatchingOptions.BatchTimeout.Milliseconds)
            .TaskReadAllAsync(cancellationToken, async batch => 
                await Project(batch, cancellationToken)
                    .ConfigureAwait(false)
            )
            .ConfigureAwait(false);

    private async Task Project(IEnumerable<EventData> eventData, CancellationToken cancellationToken)
    {
        var checkpointingOptions = _options.CheckpointingOptions;
        await using var scope = _serviceProvider.CreateAsyncScope();
        var eventTypeResolver = GetEventTypeResolver(scope.ServiceProvider);
        var projectorMethodInvoker = scope.ServiceProvider.GetRequiredService<ProjectorMethodInvoker<TProjector>>();
        var checkpointRepository = scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
        var batchPreProcessor = scope.ServiceProvider.GetService<BatchPreProcessor<TProjector>>();
        var batchPostProcessor = scope.ServiceProvider.GetService<BatchPostProcessor<TProjector>>();
        var events = eventData
            .Select(@event => new { @event.Data, type = eventTypeResolver.GetType(@event.EventName), @event.Position })
            .Select(eventData => (JsonSerializer.Deserialize(eventData.Data, eventData.type), eventData.type, eventData.Position));

        object? dependency = null;
        if (batchPreProcessor is not null)
            dependency = await batchPreProcessor
                .Invoke(cancellationToken)
                .ConfigureAwait(false);
        
        long lastEventPosition = 0;
        
        switch (dependency)
        {
            case IAsyncDisposable asyncDisposable:
            {
                await using (asyncDisposable)
                {
                    await Process()
                        .ConfigureAwait(false);
                    return;
                }
            }
            case IDisposable disposable:
            {
                using (disposable)
                {
                    await Process()
                        .ConfigureAwait(false);
                    return;
                }
            }
            case null:
            {
                await Process()
                    .ConfigureAwait(false);
                return;
            }
        }

        async Task Process()
        {
            lastEventPosition = await ProjectEvents(
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
            
            switch (checkpointingOptions.Strategy)
            {
                case CheckpointingStrategy.EveryEvent:
                case CheckpointingStrategy.Interval when _eventsProcessedSinceCheckpoint % checkpointingOptions.CheckPointingInterval == 0:
                    await SaveCheckpoint(checkpointRepository, position, cancellationToken)
                        .ConfigureAwait(false);
                    break;
            }
        }

        return lastEventPosition;
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        _stopping = true;
        await _eventChannel
            .CompleteAsync()
            .ConfigureAwait(false);
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
    
    private IEventTypeResolver GetEventTypeResolver(IServiceProvider serviceProvider) =>
        serviceProvider
            .GetRequiredService<EventTypeResolverProvider>()
            .GetEventTypeResolver(
                _options.SerializationOptions.EventTypeResolver,
                _options.SerializationOptions.Casing,
                _options.SerializationOptions.CustomEventTypeResolverType,
                EventTypes
            );
}