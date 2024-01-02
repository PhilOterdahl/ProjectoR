using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Registration;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_a_projector_without_a_valid_project_method
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test1";
    }
    
    
    [Fact]
    public void Configuration_fails()
    {
        var services = new ServiceCollection();

        var projectoRConfigurator = new ProjectoRConfigurator(services);
        
        var act = () =>  new ProjectorConfigurator<TestProjector>(projectoRConfigurator, new ProjectorOptions());;

        act
            .Should()
            .Throw<NoValidProjectMethodException>()
            .WithMessage("No valid project method for projector with projection: Test1");
    }
}