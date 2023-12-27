using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Projector;

public class ProjectorMethodInvoker<TProjector>(ProjectorInfo projectorInfo, IServiceProvider serviceProvider)
{
    public async ValueTask Invoke(
        object @event, 
        Type eventType, 
        CancellationToken cancellationToken)
    {
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var handlerInfo = projectorInfo.GetHandlerInfoForEventType(eventType);
        var method = handlerInfo.MethodInfo;
        var parameters = GenerateParameters(@event, method, cancellationToken);

        if (handlerInfo.IsAsync)
            await InvokeMethodAsync(method, projector, parameters);
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
            await method.InvokeAsync(null, parameters);
        else
            await method.InvokeAsync(projector, parameters);
    }
    
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
                var parameter = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                parameters[parameterInfo.Position] = parameter;
            }
        }

        return parameters;
    }
}