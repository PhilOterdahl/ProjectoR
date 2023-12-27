using System.Reflection;

namespace ProjectoR.Core.Projector.Batching;

public class BatchPreProcessorInfo
{
    public enum InvocationReturnType
    {
        Disposable,
        AsyncDisposable,
        Void
    };
    
    public const string BatchProcessingStarting = "BatchProcessingStarting";
    public const string PreProcess = "PreProcess";
    
    public static readonly string[] ValidMethods = {
        BatchProcessingStarting,
        PreProcess
    };
    
    public MethodInfo MethodInfo { get; }
    public Type HandlerType { get; }
    public InvocationReturnType ReturnType { get; }
    
    public bool IsAsync { get; }
    public bool IsStatic { get; }

    private static readonly Type[] ValidReturnTypes = {
        typeof(void),
        typeof(ValueTask),
        typeof(Task),
        typeof(IDisposable),
        typeof(IAsyncDisposable),
        typeof(Task<IDisposable>),
        typeof(Task<IAsyncDisposable>),
        typeof(ValueTask<IDisposable>),
        typeof(ValueTask<IAsyncDisposable>),
    };
    
    public BatchPreProcessorInfo(Type handlerType, MethodInfo methodInfo)
    {
        if (!ValidReturnTypes.Contains(methodInfo.ReturnType))
            throw new InvalidOperationException($"Projector method: {methodInfo.Name} needs to return one of the following valid return types: void, task or valueTask");
        
        HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        IsAsync = methodInfo.ReturnType.IsAwaitable();
        IsStatic = methodInfo.IsStatic;
        ReturnType = GetHandlerReturnType(methodInfo.ReturnType);
    }
    
    private static InvocationReturnType GetHandlerReturnType(Type returnType)
    {
        if (IsTaskWithDisposable(returnType))
            return InvocationReturnType.Disposable;
        
        if (IsTaskWithAsyncDisposable(returnType))
            return InvocationReturnType.AsyncDisposable;

        return InvocationReturnType.Void;
    } 
    
    private static bool IsTaskWithDisposable(Type returnType)
    {
        if (!IsTaskWithReturnValue(returnType)) 
            return false;

        var returnTypeValue = returnType
            .GenericTypeArguments
            .First();
        
        return returnTypeValue.IsAssignableTo(typeof(IDisposable));
    }
    
    private static bool IsTaskWithAsyncDisposable(Type returnType)
    {
        if (!IsTaskWithReturnValue(returnType)) 
            return false;

        var returnTypeValue = returnType
            .GenericTypeArguments
            .First();
        
        return returnTypeValue.IsAssignableTo(typeof(IAsyncDisposable));
    }

    private static bool IsTaskWithReturnValue(Type returnType)
    {
        if (!returnType.IsGenericType)
            return false;

        var genericTypeDefinition = returnType.GetGenericTypeDefinition();

        return genericTypeDefinition == typeof(Task<>) || genericTypeDefinition == typeof(ValueTask<>);
    }
}