using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Examples.Common.Processes.LoadStudent;

public static class LoadStudentRegistration
{
    public static IServiceCollection AddLoadStudentHandler(this IServiceCollection services) =>
        services
            .AddScoped<LoadStudent.Handler>();
    
    public static IEndpointRouteBuilder UseGetStudentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "students/{id}",
                async (
                    string id,
                    [FromServices]LoadStudent.Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await handler.Handle(
                        new LoadStudent.Query(id),
                        cancellationToken
                    );

                    return result is not null
                        ? TypedResults.Ok(result)
                        : Results.NotFound();
                })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetStudent")
            .WithTags("Students")
            .WithOpenApi();
        
        return endpoints;
    }
}