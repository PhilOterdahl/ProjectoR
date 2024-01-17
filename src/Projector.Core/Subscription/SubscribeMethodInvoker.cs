using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Subscription;

internal sealed class SubscribeMethodInvoker(CustomSubscriptionInfo subscriptionInfo, IServiceProvider serviceProvider)
{
    public async IAsyncEnumerable<EventData> Invoke(
        long? checkpoint,
        string[] eventNames, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var subscription = serviceProvider.GetRequiredService(subscriptionInfo.SubscriptionType);
        var subscribeInfo = subscriptionInfo.SubscribeInfo;
        var method = subscribeInfo.MethodInfo;
        var parameters = GenerateParameters(method, checkpoint, eventNames, cancellationToken);

        if (subscribeInfo.IsAsync)
        {
            var collection = InvokeMethodAsync(method, subscription, parameters, cancellationToken)
                .ConfigureAwait(false);
            await foreach (var @event in collection)
                yield return @event;
        }
        else
        {
            var collection = Invoke(method, subscription, parameters);
            foreach (var @event in collection)
                yield return @event;
        }
    }
    
    private static IEnumerable<EventData> Invoke(
        MethodInfo method, 
        object subscription,
        object[] parameters)
    {
        var collection = method.IsStatic
            ? method.Invoke(null, parameters)
            : method.Invoke(subscription, parameters);

        foreach (var messageToPublish in (IEnumerable<EventData>)collection!)
            yield return messageToPublish;
    }
    
    private static async IAsyncEnumerable<EventData> InvokeMethodAsync(
        MethodInfo method, 
        object subscription, 
        object[] parameters,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var collection = method.IsStatic
            ? method.Invoke(null, parameters)
            : method.Invoke(subscription, parameters);
        
        await foreach (var messageToPublish in ((IAsyncEnumerable<EventData>)collection!).WithCancellation(cancellationToken))
            yield return messageToPublish;
    }
    
    private object[] GenerateParameters(
        MethodInfo method,
        long? checkpoint,
        string[] eventNames,
        CancellationToken cancellationToken)
    {
        var parametersNeeded = method.GetParameters();
        var parameters = new object[parametersNeeded.Length];

        foreach (var parameterInfo in parametersNeeded)
        {
            if (parameterInfo.ParameterType == typeof(CancellationToken))
                parameters[parameterInfo.Position] = cancellationToken;
            else if (parameterInfo.ParameterType == typeof(long) && parameterInfo.Name == "checkPoint")
                parameters[parameterInfo.Position] = checkpoint;
            else if (parameterInfo.ParameterType == typeof(string[]) && parameterInfo.Name == "eventNames")
                parameters[parameterInfo.Position] = eventNames;
            else
            {
                var parameter = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                parameters[parameterInfo.Position] = parameter;
            }
        }

        return parameters;
    }
}