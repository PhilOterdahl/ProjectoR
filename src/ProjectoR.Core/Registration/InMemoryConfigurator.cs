using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;

namespace ProjectoR.Core.Registration;

internal class InMemoryConfigurator(IProjectoRConfigurator projectoRConfigurator) : IInMemoryConfigurator
{
    public IInMemoryConfigurator UseInMemoryCheckpointing()
    {
        projectoRConfigurator
            .Services
            .AddScoped<ICheckpointRepository, InMemoryCheckpointRepository>();
        
        return this;
    }
}

public interface IInMemoryConfigurator
{
    IInMemoryConfigurator UseInMemoryCheckpointing();
}