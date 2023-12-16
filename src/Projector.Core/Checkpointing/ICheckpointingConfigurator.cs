using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Checkpointing;

public interface ICheckpointingConfigurator
{
    void RegisterCheckpointing(IServiceCollection services);
}