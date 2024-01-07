using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Open.ChannelExtensions;
using ProjectoR.Core.Registration;

namespace ProjectoR.Core.Projector;

internal class ProjectorWorkQueue(ProjectorROptions options, IServiceProvider serviceProvider) : BackgroundService
{
    private record Work(string ProjectionName, IEnumerable<EventData> Events, ProjectorPriority Priority);

    private readonly Channel<Work> _queue = Channel.CreateBounded<Work>(new BoundedChannelOptions(10000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        AllowSynchronousContinuations = false,
    });
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await ProcessQueue(stoppingToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _queue.CompleteAsync();
        await base.StopAsync(cancellationToken);
    }

    public async Task QueueWork(
        string projectionName, 
        IEnumerable<EventData> events, 
        ProjectorPriority priority,
        CancellationToken cancellationToken) =>
        await _queue.Writer.WriteAsync(new Work(projectionName, events, priority), cancellationToken);

    private async Task ProcessQueue(CancellationToken cancellationToken) =>
        await _queue
            .Reader
            .Batch(100, true)
            .WithTimeout(options.PrioritizationTime)
            .Pipe(
                1,
                list => list
                    .GroupBy(item => (item.ProjectionName, item.Priority))
                    .OrderByDescending(item => item.Key.Priority),
                cancellationToken: cancellationToken
            )
            .TaskReadAllAsync(
                async workGroupedByProjector =>
                {
                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = options.MaxConcurrency
                    };

                    await Parallel.ForEachAsync(
                        workGroupedByProjector,
                        parallelOptions,
                        async (projectorWork, ct) => await ProjectEvents(projectorWork, ct)
                    );
                },
                cancellationToken
            );

    private async Task ProjectEvents(IEnumerable<Work> work, CancellationToken cancellationToken)
    {
        foreach (var (projectionName, events, _) in work)
        {
            var projectorService = serviceProvider.GetRequiredKeyedService<IProjectorService>(projectionName);
            
            try
            {
                await projectorService.Project(events, cancellationToken);
            }
            catch (Exception e)
            {
               // TODO stop projector and subscription but continue with sucessful projectors
                throw;
            }
        }
    }
}