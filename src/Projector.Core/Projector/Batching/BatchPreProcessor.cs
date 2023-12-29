using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Projector.Batching;

public class BatchPreProcessor<TProjector>(BatchPreProcessorInfo handlerInfo, IServiceProvider serviceProvider)
{
    public async ValueTask<object?> Invoke(CancellationToken cancellationToken)
    {
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var method = handlerInfo.MethodInfo;
        var parameters = GenerateParameters(method, cancellationToken);

        return handlerInfo.IsAsync
            ? await InvokeMethodAsync(method, projector, parameters)
                .ConfigureAwait(false)
            : BatchPreProcessor<TProjector>.Invoke(method, projector, parameters);
    }

    private static object? Invoke(
        MethodInfo method, 
        TProjector projector,
        object[] parameters) =>
        method.IsStatic
            ? method.Invoke(null, parameters)
            : method.Invoke(projector, parameters);

    private static async Task<object?> InvokeMethodAsync(
        MethodInfo method, 
        TProjector projector, 
        object[] parameters) =>
        method.IsStatic
            ? await method
                .InvokeWithResultAsync(null, parameters)
                .ConfigureAwait(false)
            : await method
                .InvokeWithResultAsync(projector, parameters)
                .ConfigureAwait(false);

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