using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Examples.Common.Processes.EnrollStudent;

public static class EnrollStudentRegistration
{
    public static IServiceCollection AddEnrollStudentHandler(this IServiceCollection services) =>
        services
            .AddScoped<EnrollStudent.Handler>();
    
    public static IEndpointRouteBuilder UseEnrollStudentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "students",
                async (
                    [FromBody]EnrollStudentRequest request,
                    [FromServices]EnrollStudent.Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await handler.Handle(
                        new EnrollStudent.Command(
                            request.FirstName,
                            request.LastName,
                            request.Email,
                            request.Mobile,
                            request.CountryCode,
                            request.City,
                            request.PostalCode,
                            request.Street
                        ),
                        cancellationToken
                    );

                    return Results.Created("GetStudent", result);
                })
            .Produces(StatusCodes.Status201Created)
            .WithName("EnrollStudent")
            .WithTags("Students")
            .WithOpenApi();
        
        return endpoints;
    }
}