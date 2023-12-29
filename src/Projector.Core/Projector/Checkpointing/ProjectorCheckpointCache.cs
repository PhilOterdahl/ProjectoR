using ProjectoR.Core.Checkpointing;

namespace ProjectoR.Core.Projector.Checkpointing;

internal sealed class ProjectorCheckpointCache
{
    public Checkpoint? Checkpoint { get; private set; }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        Checkpoint = checkpoint;
    }
}