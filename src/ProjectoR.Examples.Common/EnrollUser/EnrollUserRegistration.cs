using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Examples.Common.EnrollUser;

public static class EnrollUserRegistration
{
    public static IServiceCollection AddEnrollUserHandler(this IServiceCollection services) =>
        services
            .AddScoped<EnrollUser.Handler>();
    
    public static IEndpointRouteBuilder UseEnrollUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "users",
                async (
                    EnrollUser.Handler handler,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await handler.Handle(
                        new EnrollUser.Command(),
                        cancellationToken
                    );

                    return Results.Ok(result);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("Users")
            .WithTags("Users")
            .WithOpenApi();
        
        return endpoints;
    }
}