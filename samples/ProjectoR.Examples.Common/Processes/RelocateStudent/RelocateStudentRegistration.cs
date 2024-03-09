using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Examples.Common.Processes.RelocateStudent;

public static class RelocateStudentRegistration
{
    public static IServiceCollection AddRelocateStudentHandler(this IServiceCollection services) =>
        services
            .AddScoped<RelocateStudent.Handler>();
    
    public static IEndpointRouteBuilder UseRelocateStudentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "students/{id}/relocate",
                async (
                    string id,
                    [FromBody]RelocateStudentRequest request,
                    [FromServices]RelocateStudent.Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    await handler.Handle(
                        new RelocateStudent.Command(
                            id, 
                            request.CountryCode, 
                            request.City, 
                            request.PostalCode, 
                            request.Street
                        ),
                        cancellationToken
                    );

                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .WithName("RelocateStudent")
            .WithTags("Students")
            .WithOpenApi();
        
        return endpoints;
    }
}