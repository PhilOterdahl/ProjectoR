using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Projector;
using ProjectoR.Core.Projector.Serialization;
using ProjectoR.Core.Subscription;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Core.Registration;

public interface IProjectoRConfigurator
{
    int MaxConcurrency { get; set; }
    int PrioritizationBatchSize { get; set; }
    TimeSpan PrioritizationTime { get; set; }
    ProjectorSerializationOptions SerializationOptions { get; }
    IServiceCollection Services { get; }
}

public class ProjectoRConfigurator : IProjectoRConfigurator
{
    public ProjectoRConfigurator(IServiceCollection services)
    {
        PrioritizationTime = TimeSpan.FromMilliseconds(100);
        Services = services
            .AddSingleton<EventTypeResolverProvider>()
            .AddHostedService<SubscriptionWorker>()
            .AddSingleton<ProjectorWorkQueue>()
            .AddHostedService<ProjectorWorkQueue>(provider => provider.GetRequiredService<ProjectorWorkQueue>())
            .AddSingleton(new ConfiguredProjectors());
    }

    public int MaxConcurrency { get; set; } = Environment.ProcessorCount * 2;
    public int PrioritizationBatchSize { get; set; } = 100;
    public TimeSpan PrioritizationTime { get; set; }
    public IServiceCollection Services { get; }
    public ProjectorSerializationOptions SerializationOptions { get; } = new();

    public void Build()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxConcurrency, nameof(MaxConcurrency));

        var options = new ProjectorROptions
        {
            MaxConcurrency = MaxConcurrency,
            PrioritizationTime = PrioritizationTime,
            PrioritizationBatchSize = PrioritizationBatchSize
        };
        Services.AddSingleton(options);
    }
}