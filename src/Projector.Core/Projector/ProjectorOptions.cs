using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.Projector.Serialization;

namespace ProjectoR.Core.Projector;

public class ProjectorOptions
{
    public ProjectorSerializationOptions SerializationOptions { get; } = new();
    public ProjectorCheckpointingOptions CheckpointingOptions { get; } = new();
    public ProjectorBatchingOptions BatchingOptions { get; } = new();
}