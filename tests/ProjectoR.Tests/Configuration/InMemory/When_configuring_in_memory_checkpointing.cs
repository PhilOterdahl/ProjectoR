using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.InMemory;

public class When_configuring_in_memory_checkpointing
{
    [Fact]
    public void In_memory_checkpoint_repository_is_configured()
    {
        var services = new ServiceCollection();
        var projectoRConfigurator = new ProjectoRConfigurator(services);
        var inMemoryConfigurator = new InMemoryConfigurator(projectoRConfigurator);

        inMemoryConfigurator.UseInMemoryCheckpointing();
        
        services
            .Should()
            .Contain(descriptor => descriptor.ServiceType == typeof(ICheckpointRepository) && descriptor.ImplementationType == typeof(InMemoryCheckpointRepository));
    }
}