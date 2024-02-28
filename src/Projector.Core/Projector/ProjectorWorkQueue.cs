using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Open.ChannelExtensions;
using ProjectoR.Core.Registration;
using ProjectoR.Core.Subscription;

namespace ProjectoR.Core.Projector;

internal class ProjectorWorkQueue(
    ProjectorROptions options, 
    IServiceProvider serviceProvider, 
    ILogger<ProjectorWorkQueue> logger) : BackgroundService
{
    private record Work(string ProjectionName, ProjectorPriority Priority, IEnumerable<EventData> Events);
    
    private readonly Channel<Work> _workQueue = Channel.CreateBounded<Work>(
        new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });

    private BatchingChannelReader<Work, List<Work>> _workReader = null!;
    private bool _processing;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _workReader = _workQueue
            .Reader
            .Batch(1000, true, true);
            // .WithTimeout(TimeSpan.FromMilliseconds(500));
        
        await ProcessQueue(stoppingToken)
            .ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _workQueue.CompleteAsync();
        
        await base
            .StopAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Enqueue(
        string projectionName, 
        IEnumerable<EventData> events, 
        ProjectorPriority priority,
        CancellationToken cancellationToken)
    {
        await _workQueue.Writer.WriteAsync(new Work(projectionName, priority, events), cancellationToken);
        
        if (_processing)
            return;
        
        _workReader.ForceBatch();
    }

    private async Task ProcessQueue(CancellationToken cancellationToken)
    {
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = options.MaxConcurrency
        };

        await _workReader.TaskReadAllAsync(async workQueue =>
        {
            _processing = true;
            
            var workGroupedOnProjector = workQueue
                .GroupBy(queue => (queue.ProjectionName, queue.Priority))
                .OrderByDescending(item => item.Key.Item2)
                .ToArray();

            await Parallel.ForEachAsync(
                workGroupedOnProjector,
                parallelOptions,
                async (projectorWork, ct) =>
                {
                    var projectorService = serviceProvider
                        .GetRequiredKeyedService<IProjectorService>(projectorWork.Key.Item1);

                    if (projectorService.Stopped)
                        return;

                    try
                    {
                        foreach (var work in projectorWork)
                            await Project(projectorService, work.Events, ct);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception,
                            "Error when projecting for projection: {projectionName}, stopping subscription",
                            projectorService.ProjectionName
                        );
            
                        var subscription =
                            serviceProvider.GetRequiredKeyedService<IProjectionSubscription>(projectorService.ProjectionName);
            
                        await subscription
                            .Stop(cancellationToken)
                            .ConfigureAwait(false);
                    }
                   
                });

            if (_workQueue.Reader.TryPeek(out _))
            {
                _workReader.ForceBatch();
                return;
            }
            
            _processing = false;
        }, cancellationToken);
    }
    private static async ValueTask Project(
        IProjectorService projectorService, 
        IEnumerable<EventData> events, 
        CancellationToken cancellationToken)
    {
        await projectorService
            .Project(events, cancellationToken)
            .ConfigureAwait(false);
    }
}