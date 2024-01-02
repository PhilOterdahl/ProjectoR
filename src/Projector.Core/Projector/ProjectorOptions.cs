using ProjectoR.Core.Projector.Batching;
using ProjectoR.Core.Projector.Checkpointing;
using ProjectoR.Core.Projector.Serialization;

namespace ProjectoR.Core.Projector;

public sealed class ProjectorOptions(ProjectorSerializationOptions? serializationOptions = null)
{
    public ProjectorSerializationOptions SerializationOptions { get; } = serializationOptions ?? new ProjectorSerializationOptions();
    public ProjectorCheckpointingOptions CheckpointingOptions { get; } = new();
    public ProjectorBatchingOptions BatchingOptions { get; } = new();
}