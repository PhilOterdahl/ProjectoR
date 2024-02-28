namespace ProjectoR.Core.Projector.Metrics;

internal class ProjectorMetrics : IProjectorMetrics
{
    public int EventsProcessedSinceStartedRunning { get; set; }
    public int EventsPerMinute { get;  set; }
}