using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Projector;

internal sealed class ProjectorMethodInvoker<TProjector>(ProjectorInfo projectorInfo, IServiceProvider serviceProvider)
{
    public async ValueTask Invoke(
        object @event, 
        Type eventType, 
        object? dependency,
        CancellationToken cancellationToken)
    {
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var handlerInfo = projectorInfo.GetHandlerInfoForEventType(eventType);
        var method = handlerInfo.MethodInfo;
        var parameters = GenerateParameters(@event, method, dependency, cancellationToken);

        if (handlerInfo.IsAsync)
            await InvokeMethodAsync(method, projector, parameters)
                .ConfigureAwait(false);
        else
            Invoke(method, projector, parameters);
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
            await method
                .InvokeAsync(null, parameters)
                .ConfigureAwait(false);
        else
            await method
                .InvokeAsync(projector, parameters)
                .ConfigureAwait(false);
    }
    
    private object[] GenerateParameters(
        object @event,
        MethodInfo method,
        object? dependency,
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
            else if (dependency is not null && dependency.GetType().IsAssignableTo(parameterInfo.ParameterType))
                parameters[parameterInfo.Position] = dependency;
            else
            {
                var parameter = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                parameters[parameterInfo.Position] = parameter;
            }
        }

        return parameters;
    }
}