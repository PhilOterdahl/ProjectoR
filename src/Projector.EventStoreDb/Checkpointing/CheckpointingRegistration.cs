using Microsoft.Extensions.DependencyInjection;
using Projector.Core.Checkpointing;

namespace Projector.EventStoreDB.Checkpointing;

public static class CheckpointingRegistration
{
    public static IServiceCollection AddEventStoreDBCheckpointing(this IServiceCollection services) =>
        services
            .AddScoped<ICheckpointRepository, EventStoreDBCheckpointRepository>();
}