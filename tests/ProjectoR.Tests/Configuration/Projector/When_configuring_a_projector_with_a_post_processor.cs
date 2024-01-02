using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_a_projector_with_a_post_processor
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test1";

        public void Project(TestEvent @event)
        {
        }
        
        public void PostProcess()
        {
            
        }
    }
    
    private readonly ServiceCollection _services = [];

    public When_configuring_a_projector_with_a_post_processor()
    {
        var projectoRConfigurator = new ProjectoRConfigurator(_services);
        _ = new ProjectorConfigurator<TestProjector>(projectoRConfigurator);
    }
    
    [Fact]
    public void Batch_post_processor_is_configured()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceType == typeof(BatchPostProcessor<TestProjector>));
    }
}