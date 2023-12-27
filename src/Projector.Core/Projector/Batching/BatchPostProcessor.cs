using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Projector.Batching;

public class BatchPostProcessor<TProjector>(BatchPostProcessorInfo handlerInfo, IServiceProvider serviceProvider)
{
    public async ValueTask Invoke(CancellationToken cancellationToken)
    {
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var method = handlerInfo.MethodInfo;
        var parameters = GenerateParameters(method, cancellationToken);

        if (handlerInfo.IsAsync)
            await InvokeMethodAsync(method, projector, parameters);
        else
            Invoke(method, projector, parameters);
    }

    private void Invoke(
        MethodInfo method, 
        TProjector projector,
        object[] parameters)
    {
        if (method.IsStatic)
            method.Invoke(null, parameters);
        else 
            method.Invoke(projector, parameters);
    }
    
    private async ValueTask InvokeMethodAsync(
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
        MethodInfo method,
        CancellationToken cancellationToken)
    {
        var parametersNeeded = method.GetParameters();
        var parameters = new object[parametersNeeded.Length];

        foreach (var parameterInfo in parametersNeeded)
        {
            if (parameterInfo.ParameterType == typeof(CancellationToken))
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