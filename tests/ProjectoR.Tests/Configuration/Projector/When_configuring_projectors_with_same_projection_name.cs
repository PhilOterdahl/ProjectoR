using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_projectors_with_same_projection_name
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test";

        public void Project(TestEvent @event)
        {
        }
    }

    public class TestProjector2
    {
        public static string ProjectionName => "Test";
        
        public void Project(TestEvent @event)
        {
        }
    }
    
    [Fact]
    public void Configuration_fails()
    {
        var services = new ServiceCollection();

        var projectoRConfigurator = new ProjectoRConfigurator(services);
        var projectorConfigurator = new ProjectorConfigurator<TestProjector>(projectoRConfigurator);
        
        var act = () =>  new ProjectorConfigurator<TestProjector2>(projectoRConfigurator);

        act
            .Should()
            .Throw<ProjectionNameNotUniqueException>()
            .WithMessage("Projector with projectionName: Test already exists, ProjectionName needs to be unique");
    }
}