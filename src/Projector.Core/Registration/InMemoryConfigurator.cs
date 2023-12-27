using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;

namespace ProjectoR.Core.Registration;

public class InMemoryConfigurator(IServiceCollection services) : IInMemoryConfigurator
{
    public IInMemoryConfigurator UseInMemoryCheckpointing()
    {
        services
            .AddScoped<ICheckpointRepository, InMemoryCheckpointRepository>();
        
        return this;
    }
}

public interface IInMemoryConfigurator
{
    IInMemoryConfigurator UseInMemoryCheckpointing();
}