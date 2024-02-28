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
        typeof(IEnumerable<EventData>),
        typeof(IAsyncEnumerable<EventData>),
    };
    
    public SubscribeInfo(Type subscriptionType, MethodInfo methodInfo)
    {
        if (!ValidReturnTypes.Contains(methodInfo.ReturnType))
            throw new InvalidOperationException("Subscribe method needs to return one of the following valid return types: IEnumerable<EventData> or IAsyncEnumerable<EventData>");
        
        SubscriptionType = subscriptionType ?? throw new ArgumentNullException(nameof(subscriptionType));
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        IsAsync = IsAwaitable(methodInfo.ReturnType);
        IsStatic = methodInfo.IsStatic;
        ReturnType = methodInfo.ReturnType;
    }
    
    private static bool IsAwaitable(Type returnType) => returnType == typeof(IAsyncEnumerable<EventData>);
}