using System.Reflection;

namespace ProjectoR.Core.Projector.Batching;

public class BatchPreProcessorInfo(Type handlerType, MethodInfo methodInfo)
{
    public const string BatchProcessingStarting = "BatchProcessingStarting";
    public const string PreProcess = "PreProcess";
    
    public static readonly string[] ValidMethods = {
        BatchProcessingStarting,
        PreProcess
    };
    
    public MethodInfo MethodInfo { get; } = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
    public Type HandlerType { get; } = handlerType ?? throw new ArgumentNullException(nameof(handlerType));

    public bool IsAsync { get; } = methodInfo.ReturnType.IsAwaitable();
    public bool IsStatic { get; } = methodInfo.IsStatic;
}