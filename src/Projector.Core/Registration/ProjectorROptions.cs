namespace ProjectoR.Core.Registration;

internal class ProjectorROptions
{
    public int MaxConcurrency { get; set; }
    public int PrioritizationBatchSize { get; set; }
    public TimeSpan PrioritizationTime { get; set; }
}