using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Serialization;
using ProjectoR.Core.Registration;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Tests.Configuration.Projector;

public class When_configuring_a_projector_with_a_custom_event_type_resolver
{
    public record TestEvent();

    public class TestProjector
    {
        public static string ProjectionName => "Test1";

        public void Project(TestEvent @event)
        {
        }
    }
    
    public class CustomEventTypeResolver : IEventTypeResolver
    {
        public void SetEventTypes(IEnumerable<Type> eventTypes)
        {
            throw new NotImplementedException();
        }

        public Type GetType(string eventName)
        {
            throw new NotImplementedException();
        }

        public string GetName(Type eventType)
        {
            throw new NotImplementedException();
        }
    }
    
    private readonly ServiceCollection _services = [];

    public When_configuring_a_projector_with_a_custom_event_type_resolver()
    {
        var projectoRConfigurator = new ProjectoRConfigurator(_services);
     
        _ = new ProjectorConfigurator<TestProjector>(projectoRConfigurator, projectorOptions =>
        {
            projectorOptions.SerializationOptions.UseCustomEventTypeResolver<CustomEventTypeResolver>();
        });
    }
    
    [Fact]
    public void Custom_event_type_resolver_is_registered()
    {
        _services
            .Should()
            .Contain(descriptor => descriptor.ServiceKey == "Test1" && 
                                   descriptor.ServiceType == typeof(IEventTypeResolver) &&
                                   descriptor.KeyedImplementationType == typeof(CustomEventTypeResolver)
            );
    }
    
    [Fact]
    public void Projector_options_event_type_resolver_type_is_set_to_custom()
    {
        _services
            .BuildServiceProvider()
            .GetRequiredKeyedService<ProjectorOptions>("Test1")
            .SerializationOptions
            .EventTypeResolver
            .Should()
            .Be(EventTypeResolverType.Custom);
    }
}