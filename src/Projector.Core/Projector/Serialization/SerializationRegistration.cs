using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectoR.Core.EventNameFormatters;
using ProjectoR.Core.Registration;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Projector.Serialization;

internal static class SerializationRegistration
{
    public static IProjectoRConfigurator RegisterEventTypeResolver(this IProjectoRConfigurator projectoRConfigurator, ProjectorInfo projectorInfo) =>
        projectorInfo.Options.SerializationOptions.EventTypeResolver switch
        {
            EventTypeResolverType.Namespace => RegisterNameSpaceEventTypeResolver(
                projectoRConfigurator,
                projectorInfo
            ),
            EventTypeResolverType.ClassName => RegisterClassNameEventTypeResolver(
                projectoRConfigurator,
                projectorInfo
            ),
            EventTypeResolverType.Custom => RegisterCustomEventTypeResolver(projectoRConfigurator, projectorInfo)
        };

    private static IProjectoRConfigurator RegisterCustomEventTypeResolver(IProjectoRConfigurator projectoRConfigurator, ProjectorInfo projectorInfo)
    {
        if (!IsUsingDefaultEventTypeResolver(projectoRConfigurator, projectorInfo.Options))   
            projectoRConfigurator.Services.TryAddKeyedScoped(typeof(IEventTypeResolver), projectorInfo.ProjectionName, projectorInfo.Options.SerializationOptions.CustomEventTypeResolverType!);
        else if (!EventTypeResolverAlreadyRegistered(projectoRConfigurator))
            projectoRConfigurator.Services.AddScoped(typeof(IEventTypeResolver), projectorInfo.Options.SerializationOptions.CustomEventTypeResolverType!);

        return projectoRConfigurator;
    }

    private static IProjectoRConfigurator RegisterNameSpaceEventTypeResolver(
        IProjectoRConfigurator configurator,
        ProjectorInfo projectorInfo)
    {
        var resolver = new NameSpaceEventTypeResolver(
            GetEventNameFormatter(projectorInfo.Options.SerializationOptions.Casing),
            projectorInfo.EventTypes
        );
        configurator.Services.AddKeyedSingleton<IEventTypeResolver>(projectorInfo.ProjectionName, resolver);
        return configurator;
    }
    
    private static IProjectoRConfigurator RegisterClassNameEventTypeResolver(
        IProjectoRConfigurator configurator, 
        ProjectorInfo projectorInfo)
    {
        var resolver = new ClassNameEventTypeResolver(
            GetEventNameFormatter(projectorInfo.Options.SerializationOptions.Casing), 
            projectorInfo.EventTypes
        );
        configurator.Services.AddKeyedSingleton<IEventTypeResolver>(projectorInfo.ProjectionName, resolver);
        return configurator;
    }
    
    private static IEventNameFormatter GetEventNameFormatter(EventTypeResolverCasing casing) =>
        casing switch
        {
            EventTypeResolverCasing.CamelCase => new CamelCaseEventNameFormatter(),
            EventTypeResolverCasing.KebabCase => new KebabCaseEventNameFormatter(),
            EventTypeResolverCasing.PascalCase => new PascalCaseEventNameFormatter(),
            EventTypeResolverCasing.SnakeCase => new SnakeCaseEventNameFormatter(),
            _ => throw new ArgumentOutOfRangeException(nameof(casing), casing, null)
        };
    
    private static bool IsUsingDefaultEventTypeResolver(IProjectoRConfigurator projectoRConfigurator, ProjectorOptions options) => 
        projectoRConfigurator.SerializationOptions.CustomEventTypeResolverType == options.SerializationOptions.CustomEventTypeResolverType;

    private static bool EventTypeResolverAlreadyRegistered(IProjectoRConfigurator projectoRConfigurator) => 
        projectoRConfigurator.Services.Any(descriptor => descriptor.ServiceType == typeof(IEventTypeResolver));
}