using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_a_projector_with_a_project_method_with_out_parameters
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test1";

        public void Project()
        {
        }
    }
    
    
    [Fact]
    public void Configuration_fails()
    {
        var services = new ServiceCollection();

        var projectoRConfigurator = new ProjectoRConfigurator(services);
        
        var act = () =>  new ProjectorConfigurator<TestProjector>(projectoRConfigurator, new ProjectorOptions());;

        act
            .Should()
            .Throw<ProjectMethodWithOutParametersException>()
            .WithMessage($"Projector with projection: Test1 has a project method without parameters, first parameters needs to be the event");
    }
}