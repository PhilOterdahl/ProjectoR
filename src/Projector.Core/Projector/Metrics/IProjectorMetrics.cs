namespace ProjectoR.Core.Projector.Metrics;

public interface IProjectorMetrics
{
    public int EventsProcessedSinceStartedRunning { get; }
    public int EventsPerMinute { get; } 
}