namespace ProjectoR.Core.Subscription;

internal sealed class CustomSubscriptionInfo(string projectionName, Type subscriptionType)
{
    public string ProjectionName { get; } = projectionName;
    public Type SubscriptionType { get;  } = subscriptionType;
    public SubscribeInfo SubscribeInfo { get; } = GetSubscribeInfo(subscriptionType);

    private static readonly string[] ValidMethods = {
        "Subscribe"
    };

    private static SubscribeInfo GetSubscribeInfo(Type subscriptionType)
    {
        var subscribeInfo = subscriptionType
            .GetMethods()
            .Where(info => ValidMethods.Contains(info.Name))
            .Select(info => new SubscribeInfo(subscriptionType, info))
            .ToArray();

        return subscribeInfo.Length switch
        {
            > 1 => throw new InvalidOperationException("Multiple subscribe methods"),
            0 => throw new InvalidOperationException("No subscribe method found"),
            _ => subscribeInfo.Single()
        };
    }
}