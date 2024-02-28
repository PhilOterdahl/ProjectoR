using System.Reflection;

namespace ProjectoR.Core.Projector.Batching;

internal sealed class BatchPostProcessorInfo
{
    public const string BatchProcessingCompleted = "BatchProcessingCompleted";
    public const string PostProcess = "PostProcess";
    
    public static readonly string[] ValidMethods = {
        BatchProcessingCompleted,
        PostProcess
    };  
    
    public MethodInfo MethodInfo { get; }
    public Type HandlerType { get; }
    public Type ReturnType { get; }
    
    public bool IsAsync { get; }
    public bool IsStatic { get; }

    private static readonly Type[] ValidReturnTypes = {
        typeof(void),
        typeof(ValueTask),
        typeof(Task),
    };
    
    public BatchPostProcessorInfo(Type handlerType, MethodInfo methodInfo)
    {
        if (!ValidReturnTypes.Contains(methodInfo.ReturnType))
            throw new InvalidOperationException($"Batch post processor method: {methodInfo.Name} needs to return one of the following valid return types: void, task or valueTask");
        
        HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        IsAsync = methodInfo.ReturnType.IsAwaitable();
        IsStatic = methodInfo.IsStatic;
        ReturnType = methodInfo.ReturnType;
    }
}