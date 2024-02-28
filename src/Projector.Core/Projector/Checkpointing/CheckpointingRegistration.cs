using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Registration;

namespace ProjectoR.Core.Projector.Checkpointing;

internal static class CheckpointingRegistration
{
    public static IProjectoRConfigurator RegisterCheckpointing(this IProjectoRConfigurator configurator, ProjectorInfo projectorInfo)
    {
        configurator.Services.AddKeyedSingleton<ProjectorCheckpointCache>(projectorInfo.ProjectionName);
        return configurator;
    }
}