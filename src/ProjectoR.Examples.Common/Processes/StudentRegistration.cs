using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Examples.Common.Processes.ChangeStudentContactInformation;
using ProjectoR.Examples.Common.Processes.EnrollStudent;
using ProjectoR.Examples.Common.Processes.GraduateStudent;
using ProjectoR.Examples.Common.Processes.LoadStudent;
using ProjectoR.Examples.Common.Processes.RelocateStudent;

namespace ProjectoR.Examples.Common.Processes;

public static class StudentRegistration
{
    public static IServiceCollection AddStudentProcesses(this IServiceCollection services) =>
        services
            .AddChangeStudentContactInformationHandler()
            .AddEnrollStudentHandler()
            .AddGraduateStudentHandler()
            .AddLoadStudentHandler()
            .AddRelocateStudentHandler();

    public static IEndpointRouteBuilder UseStudentEndpoints(this IEndpointRouteBuilder routeBuilder) =>
        routeBuilder
            .UseChangeStudentContactInformationEndpoint()
            .UseEnrollStudentEndpoint()
            .UseGraduateStudentEndpoint()
            .UseGetStudentEndpoint()
            .UseRelocateStudentEndpoint();
}