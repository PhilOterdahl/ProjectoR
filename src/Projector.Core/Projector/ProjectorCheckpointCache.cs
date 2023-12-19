using ProjectoR.Core.Checkpointing;

namespace ProjectoR.Core.Projector;

public sealed class ProjectorCheckpointCache<TProjector>
{
    public Checkpoint? Checkpoint { get; private set; }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        Checkpoint = checkpoint;
    }
}