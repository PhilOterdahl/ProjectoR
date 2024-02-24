using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Examples.Common.Processes.GraduateStudent;

public static class GraduateStudentRegistration
{
    public static IServiceCollection AddGraduateStudentHandler(this IServiceCollection services) =>
        services
            .AddScoped<GraduateStudent.Handler>();
    
    public static IEndpointRouteBuilder UseGraduateStudentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "students/{id}/graduate",
                async (
                    string id,
                    [FromServices]GraduateStudent.Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    await handler.Handle(
                        new GraduateStudent.Command(id),
                        cancellationToken
                    );

                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .WithName("GraduateStudent")
            .WithTags("Students")
            .WithOpenApi();
        
        return endpoints;
    }
}