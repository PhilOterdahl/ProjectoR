using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector;

public class ProjectorService<TProjector>
{
    private readonly ProjectorOptions _options;
    public long? Position => _checkpointCache.Checkpoint?.Position;
    private readonly ProjectorCheckpointCache<TProjector> _checkpointCache;
    private readonly ProjectorInfo _projectorInfo;
    private readonly IServiceProvider _serviceProvider;

    public Type[] EventTypes => _projectorInfo.EventTypes;
    public string[] EventNames { get; }
    public string ProjectionName { get; }
    
    public ProjectorService(IServiceProvider serviceProvider, ProjectorInfo projectorInfo)
    {
        _serviceProvider = serviceProvider;
        _options = serviceProvider.GetRequiredKeyedService<ProjectorOptions>(projectorInfo.ProjectionName);
        _checkpointCache = serviceProvider.GetRequiredService<ProjectorCheckpointCache<TProjector>>();
        _projectorInfo = projectorInfo;
        var eventTypeResolver = GetEventTypeResolver(serviceProvider);
        EventNames = EventTypes
            .Select(eventType => eventTypeResolver.GetName(eventType))
            .ToArray();
        ProjectionName = projectorInfo.ProjectionName;
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

    public async Task Project(IEnumerable<EventData> eventData, CancellationToken cancellationToken)
    {   
        await using var scope = _serviceProvider.CreateAsyncScope();
        var eventTypeResolver = GetEventTypeResolver(scope.ServiceProvider);
        var projector = scope.ServiceProvider.GetRequiredService<TProjector>();
        var checkpointRepository = scope.ServiceProvider.GetRequiredService<ICheckpointRepository>();
        
        var events = eventData
            .Select(@event => new { @event.Data, type = eventTypeResolver.GetType(@event.EventName), @event.Position })
            .Select(eventData => (JsonSerializer.Deserialize(eventData.Data, eventData.type), eventData.type, eventData.Position));
        
        foreach (var (@event, eventType, position) in events)
        {
            var handlerInfo = _projectorInfo.GetHandlerInfoForEventType(eventType);
            var method = handlerInfo.MethodInfo;
            var parameters = GenerateParameters(@event, method, cancellationToken);

            if (handlerInfo.IsAsync)
                await InvokeMethodAsync(method, projector, parameters);
            else
                Invoke(method, projector, parameters);
            
            await SaveCheckpoint(checkpointRepository, position, cancellationToken);
        }
    }

    private static void Invoke(
        MethodInfo method, 
        TProjector projector,
        object[] parameters)
    {
        if (method.IsStatic)
            method.Invoke(null, parameters);
        else
            method.Invoke(projector, parameters);
    }

    private static async Task InvokeMethodAsync(
        MethodInfo method, 
        TProjector projector, 
        object[] parameters)
    {
        if (method.IsStatic)
            await method.InvokeAsync(null, parameters);
        else
            await method.InvokeAsync(projector, parameters);
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
    }
    
    private IEventTypeResolver GetEventTypeResolver(IServiceProvider serviceProvider) =>
        serviceProvider
            .GetRequiredService<EventTypeResolverProvider>()
            .GetEventTypeResolver(
                _options.EventTypeResolver,
                _options.Casing,
                _options.CustomEventTypeResolverType,
                EventTypes
            );

    private object[] GenerateParameters(
        object @event,
        MethodInfo method,
        CancellationToken cancellationToken)
    {
        var parametersNeeded = method.GetParameters();
        var parameters = new object[parametersNeeded.Length];

        foreach (var parameterInfo in parametersNeeded)
        {
            if (parameterInfo.ParameterType == @event.GetType())
                parameters[parameterInfo.Position] = @event;
            else if (parameterInfo.ParameterType == typeof(CancellationToken))
                parameters[parameterInfo.Position] = cancellationToken;
            else
            {
                var parameter = _serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                parameters[parameterInfo.Position] = parameter;
            }
        }

        return parameters;
    }
}