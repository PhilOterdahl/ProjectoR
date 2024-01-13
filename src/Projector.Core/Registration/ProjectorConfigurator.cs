using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Serialization;
using ProjectoR.Core.Subscription;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Registration;

public interface IProjectoRConfigurator
{
    int MaxConcurrency { get; set; }
    ProjectorSerializationOptions SerializationOptions { get; }
    IServiceCollection Services { get; }
}

public class ProjectoRConfigurator : IProjectoRConfigurator
{
    public ProjectoRConfigurator(IServiceCollection services)
    {
        Services = services
            .AddHostedService<SubscriptionWorker>()
            .AddSingleton<ProjectorWorkQueue>()
            .AddHostedService<ProjectorWorkQueue>(provider => provider.GetRequiredService<ProjectorWorkQueue>())
            .AddSingleton(new ConfiguredProjectors());
    }

    public int MaxConcurrency { get; set; } = -1;
    public IServiceCollection Services { get; }
    public ProjectorSerializationOptions SerializationOptions { get; } = new();

    public void Build()
    {
        var amountOfProjectors = Services
            .BuildServiceProvider()
            .GetRequiredService<ConfiguredProjectors>()
            .Count;
        
        var options = new ProjectorROptions
        {
            MaxConcurrency = MaxConcurrency < 0 
                ? amountOfProjectors
                : MaxConcurrency,
        };
        Services.AddSingleton(options);
    }
}