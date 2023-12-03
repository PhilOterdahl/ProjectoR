namespace Projector.Checkpointing;

public class CheckpointState
{
    public required string ProjectionName { get; set; }
    public required long Position { get; set; }
}

public class Checkpoint
{
    public static Checkpoint CreateCheckpoint(string projectionName, long position) => new(projectionName, position);
    public static Checkpoint FromState(CheckpointState state) => new(state);
    
    public static implicit operator CheckpointState(Checkpoint checkpoint) => checkpoint._state;

    public string ProjectionName => _state.ProjectionName;
    public EventPosition Position => _state.Position;
    
    private readonly CheckpointState _state;

    private Checkpoint(string projectionName, EventPosition position)
    {
       ArgumentException.ThrowIfNullOrEmpty(projectionName);

       _state = new CheckpointState
       {
           ProjectionName = projectionName,
           Position = position
       };
    }

    private Checkpoint(CheckpointState state)
    {
        _state = state;
    }
    
    public Checkpoint CheckpointMade(EventPosition position)
    {
        if (position <= Position)
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "Position needs to be greater than current checkpoint position"
            );

        _state.Position = position;

        return this;
    }
}