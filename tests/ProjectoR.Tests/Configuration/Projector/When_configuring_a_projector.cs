using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_a_projector
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test1";

        public void Project(TestEvent @event)
        {
        }
    }
    
    private readonly ServiceCollection _services = [];

    public When_configuring_a_projector()
    {
        var projectoRConfigurator = new ProjectoRConfigurator(_services);
        _ = new ProjectorConfigurator<TestProjector>(projectoRConfigurator, new ProjectorOptions());
    }
    
    [Fact]
    public void Checkpoint_cache_is_configured_for_projector()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceKey == "Test1" && descriptor.ServiceType == typeof(ProjectorCheckpointCache));
    }
    
    [Fact]
    public void Projector_options_is_configured_for_projector()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceKey == "Test1" && descriptor.ServiceType == typeof(ProjectorOptions));
    }
    
    [Fact]
    public void Projector_is_configured()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceType == typeof(TestProjector));
    }
    
    [Fact]
    public void Projector_method_invoker_is_configured()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceType == typeof(ProjectorMethodInvoker<TestProjector>));
    }
    
    [Fact]
    public void Projector_service_is_configured()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceType == typeof(ProjectorService<TestProjector>));
    }
}