using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_a_projector_with_multiple_project_methods_with_same_event_type_as_parameter
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test1";

        public void Project(TestEvent  @event)
        {
        }
        
        public void Handle(TestEvent  @event)
        {
        }
    }
    
    
    [Fact]
    public void Configuration_fails()
    {
        var services = new ServiceCollection();

        var projectoRConfigurator = new ProjectoRConfigurator(services);
        
        var act = () =>  new ProjectorConfigurator<TestProjector>(projectoRConfigurator);

        act
            .Should()
            .Throw<MultipleProjectMethodsWithSameEventTypeException>()
            .WithMessage("Projector with projection: Test1 has multiple project methods with eventType: TestEvent as parameter");
    }
}