namespace ProjectoR.Core.Projector.Batching;

public sealed class ProjectorBatchingOptions
{
    public int BatchSize { get; set; } = 1000;
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
}