using System.Reflection;

namespace ProjectoR.Core.Projector;

public static class ProjectorHandlerExtensions
{
    public static bool IsAwaitable(this Type returnType) => returnType.IsValueTask() || returnType.IsTask();
    
    public static bool IsValueTask(this Type returnType) =>
        returnType == typeof(ValueTask) ||
        returnType.IsGenericType &&
        returnType.GetGenericTypeDefinition() == typeof(ValueTask<>);
    
    public static bool IsTask(this Type returnType) =>
        returnType == typeof(Task) ||
        returnType.IsGenericType &&
        returnType.GetGenericTypeDefinition() == typeof(Task<>);
    
    public static async ValueTask InvokeAsync(this MethodInfo method, object obj, params object[] parameters)
    {
        dynamic awaitable = method.Invoke(obj, parameters);
        await awaitable;
    }
    
    public static async ValueTask<object?> InvokeWithResultAsync(this MethodInfo method, object obj, params object[] parameters)
    {
        dynamic awaitable = method.Invoke(obj, parameters);
        await awaitable;
        var awaiter = awaitable.GetAwaiter();
        var result = awaiter.GetResult();
        return result ?? null;
    }
}