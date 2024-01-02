namespace ProjectoR.Core.Projector.Checkpointing;

public enum CheckpointingStrategy
{
    EveryEvent,
    Interval,
    AfterBatch
}

public sealed class ProjectorCheckpointingOptions
{
    public CheckpointingStrategy Strategy { get; private set; } = CheckpointingStrategy.EveryEvent;
    public int CheckPointingInterval { get; private set; }
    
    public ProjectorCheckpointingOptions CheckpointEveryEvent()
    {
        Strategy = CheckpointingStrategy.EveryEvent;
        return this;
    }
    
    public ProjectorCheckpointingOptions CheckpointInIntervals(int checkPointingInterval)
    {
        if (checkPointingInterval <= 0)
            throw new ArgumentOutOfRangeException(nameof(checkPointingInterval), "Checkpointing interval needs to be greater than zero");
        
        Strategy = CheckpointingStrategy.Interval;
        CheckPointingInterval = checkPointingInterval;
        return this;
    }
    
    public ProjectorCheckpointingOptions CheckpointAfterBatch()
    {
        Strategy = CheckpointingStrategy.AfterBatch;
        return this;
    }
}