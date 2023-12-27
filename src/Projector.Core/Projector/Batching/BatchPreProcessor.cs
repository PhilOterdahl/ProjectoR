using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Projector.Batching;

public class BatchPreProcessor<TProjector>(BatchPreProcessorInfo handlerInfo, IServiceProvider serviceProvider)
{
    public record InvocationResponse(IDisposable? Dependencies  = null, IAsyncDisposable? AsyncDependencies = null);
    
    public async ValueTask<InvocationResponse> Invoke(CancellationToken cancellationToken)
    {
        var projector = serviceProvider.GetRequiredService<TProjector>();
        var method = handlerInfo.MethodInfo;
        var parameters = GenerateParameters(method, cancellationToken);

        return handlerInfo.IsAsync
            ? await InvokeMethodAsync(method, projector, parameters)
            : Invoke(method, projector, parameters);
    }

    private InvocationResponse Invoke(
        MethodInfo method, 
        TProjector projector,
        object[] parameters)
    {
        var result = method.IsStatic
            ? method.Invoke(null, parameters)
            : method.Invoke(projector, parameters);
        
        return MapToInvocationResponse(result);
    }
    
    private async Task<InvocationResponse> InvokeMethodAsync(
        MethodInfo method, 
        TProjector projector, 
        object[] parameters)
    {
        var result = method.IsStatic
            ? await method.InvokeWithResultAsync(null, parameters)
            : await method.InvokeWithResultAsync(projector, parameters);

        return handlerInfo.ReturnType switch
        {
            BatchPreProcessorInfo.InvocationReturnType.Disposable => new InvocationResponse(
                (IDisposable?)result),
            BatchPreProcessorInfo.InvocationReturnType.AsyncDisposable => new InvocationResponse(
                AsyncDependencies: (IAsyncDisposable?)result),
            _ => new InvocationResponse()
        };
    }
    
    private InvocationResponse MapToInvocationResponse(object? result) =>
        handlerInfo.ReturnType switch
        {
            BatchPreProcessorInfo.InvocationReturnType.Disposable => new InvocationResponse(
                (IDisposable?)result),
            BatchPreProcessorInfo.InvocationReturnType.AsyncDisposable => new InvocationResponse(
                AsyncDependencies: (IAsyncDisposable?)result),
            _ => new InvocationResponse()
        };

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