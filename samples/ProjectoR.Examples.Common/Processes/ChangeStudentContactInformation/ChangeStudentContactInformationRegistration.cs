using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Examples.Common.Processes.ChangeStudentContactInformation;

public static class ChangeStudentContactInformationRegistration
{
    public static IServiceCollection AddChangeStudentContactInformationHandler(this IServiceCollection services) =>
        services
            .AddScoped<ChangeStudentContactInformation.Handler>();
    
    public static IEndpointRouteBuilder UseChangeStudentContactInformationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "students/{id}/contact-information",
                async (
                    string id,
                    [FromBody]ChangeStudentContactInformationRequest request,
                    [FromServices]ChangeStudentContactInformation.Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    await handler.Handle(
                        new ChangeStudentContactInformation.Command(
                            id, 
                            request.Mobile, 
                            request.Email
                        ),
                        cancellationToken
                    );

                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .WithName("ChangeStudentContactInformation")
            .WithTags("Students")
            .WithOpenApi();
        
        return endpoints;
    }
}