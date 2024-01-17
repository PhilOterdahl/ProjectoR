using System.Reflection;
using ProjectoR.Core.Projector;

namespace ProjectoR.Core.Subscription;

public class SubscribeInfo
{
    public MethodInfo MethodInfo { get; }
    public Type SubscriptionType { get; }
    public Type ReturnType { get; }
    
    public bool IsAsync { get; }
    public bool IsStatic { get; }

    private static readonly Type[] ValidReturnTypes = {
        typeof(void),
        typeof(ValueTask),
        typeof(Task),
    };
    
    public SubscribeInfo(Type subscriptionType, MethodInfo methodInfo)
    {
        if (!ValidReturnTypes.Contains(methodInfo.ReturnType))
            throw new InvalidOperationException($"Post batch processed handler method: {methodInfo.Name} needs to return one of the following valid return types: void, task or valueTask");
        
        SubscriptionType = subscriptionType ?? throw new ArgumentNullException(nameof(subscriptionType));
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        IsAsync = methodInfo.ReturnType.IsAwaitable();
        IsStatic = methodInfo.IsStatic;
        ReturnType = methodInfo.ReturnType;
    }
}