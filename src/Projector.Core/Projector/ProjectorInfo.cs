using System.Reflection;

namespace ProjectoR.Core.Projector;

public class ProjectorInfo
{
    public Type[] EventTypes { get; private set; } 
    public string ProjectionName { get; private set; }
    
    private readonly Dictionary<Type, ProjectorHandlerInfo> _handlerInfo;
    
    private static readonly string[] ValidMethods = {
        ProjectorHandlerInfo.Consume,
        ProjectorHandlerInfo.Consumes,
        ProjectorHandlerInfo.Handle,
        ProjectorHandlerInfo.Handles,
        ProjectorHandlerInfo.When
    };
    
    public ProjectorInfo(Type projectorType)
    {
        _handlerInfo = GetProjectorHandlerInfo(projectorType);
        EventTypes = _handlerInfo
            .Keys
            .ToArray();
        ProjectionName = GetProjectionName(projectorType);
    }

    public ProjectorHandlerInfo GetHandlerInfoForEventType(Type eventType) => _handlerInfo[eventType];

    private static Dictionary<Type, ProjectorHandlerInfo> GetProjectorHandlerInfo(Type projectorType)
    {
        var methods = projectorType.GetMethods();
        var handlerInfo = projectorType
            .GetMethods()
            .Where(method => ValidMethods.Contains(method.Name))
            .Select(method =>
            {
                var parameters = method.GetParameters();
                if (!parameters.Any())
                    throw new InvalidOperationException($"No message parameter for projector method: {method.Name}");
                        
                var messageType = parameters.First().ParameterType;
                
                return new ProjectorHandlerInfo(messageType, projectorType, method);
            })
            .ToArray();

        if (!handlerInfo.Any())
            throw new InvalidOperationException($"No valid projector methods found for projector: {projectorType.Name}");

        var eventTypeGroup = handlerInfo
            .GroupBy(info => info.MessageType)
            .FirstOrDefault(group => group.Count() > 1);

        var eventTypeIsUsedForMultipleProjectorMethods = eventTypeGroup is not null;
        
        if (eventTypeIsUsedForMultipleProjectorMethods)
            throw new InvalidOperationException($"Multiple methods found for projector: {projectorType.Name} for eventType: {eventTypeGroup!.Key.Name}");

        return handlerInfo.ToDictionary(info => info.MessageType);
    }

    private static string GetProjectionName(Type projectorType)
    {
        var projectionNameProperty =
            projectorType.GetProperty("ProjectionName", BindingFlags.Static | BindingFlags.Public);

        if (projectionNameProperty is null)
            throw new InvalidOperationException($"Projector: {projectorType.Name} is missing static public property ProjectionName");
                
        var projectionName = (string)projectionNameProperty.GetValue(null);

        if (string.IsNullOrWhiteSpace(projectionName))
            throw new InvalidOperationException($"ProjectionName property value for projector: {projectorType.Name} is not set to a value");

        return projectionName;
    }
}

public class ProjectorHandlerInfo
{
    public const string Consume = "Consume";
    public const string Consumes = "Consumes";
    public const string Handle = "Handle";
    public const string Handles = "Handles";
    public const string When = "When";
    
    public Type MessageType { get; }
    public Type HandlerType { get; }
    public MethodInfo MethodInfo { get; }
    
    public bool IsAsync { get; }
    public bool IsStatic { get; }

    private static readonly Type[] ValidReturnTypes = {
        typeof(void),
        typeof(ValueTask),
        typeof(Task)
    };
    
    public ProjectorHandlerInfo(Type messageType, Type handlerType, MethodInfo methodInfo)
    {
        if (!ValidReturnTypes.Contains(methodInfo.ReturnType))
            throw new InvalidOperationException($"Projector method: {methodInfo.Name} needs to return one of the following valid return types: void, task or valueTask");
        
        HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        IsAsync = methodInfo.ReturnType.IsAwaitable();
        IsStatic = methodInfo.IsStatic;
    }
    
}