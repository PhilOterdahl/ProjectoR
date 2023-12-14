using System.Text.Json;
using EventStore.Client;
using Projector.Core.Checkpointing;

namespace Projector.EventStoreDB.Checkpointing;

internal class EventStoreDBCheckpointRepository : ICheckpointRepository
{
    private readonly EventStoreClient _eventStoreClient;

    public EventStoreDBCheckpointRepository(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task<Checkpoint?> TryLoad(string projectionName, CancellationToken cancellationToken = default)
    {
        var streamName = GetCheckpointStreamName(projectionName);
        
        var result = _eventStoreClient.ReadStreamAsync(
            Direction.Backwards, 
            streamName,
            StreamPosition.End,
            1,
            cancellationToken: cancellationToken
        );
        
        if (await result.ReadState == ReadState.StreamNotFound)
            return null;

        ResolvedEvent? @event = await result.FirstOrDefaultAsync(cancellationToken);

        var state = JsonSerializer.Deserialize<CheckpointState>(@event.Value.Event.Data.ToArray());
        return Checkpoint.FromState(state);
    }

    public async Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default) =>
        await TryLoad(projectionName, cancellationToken) ?? throw new InvalidOperationException($"Checkpoint not found for projection: {projectionName}");

    public async Task MakeCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default)
    {
        var @event = new EventData(
            Uuid.NewUuid(),
            "Checkpoint",
            JsonSerializer.SerializeToUtf8Bytes((CheckpointState)checkpoint)
        );
        
        var eventToAppend = new[] { @event };
        var streamName = GetCheckpointStreamName(checkpoint.ProjectionName);

        try
        {
            // store new checkpoint expecting stream to exist
            await _eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.Any,
                eventToAppend,
                cancellationToken: cancellationToken
            );
        }
        catch (WrongExpectedVersionException)
        {
            // WrongExpectedVersionException means that stream did not exist
            // Set the checkpoint stream to have at most 1 event
            // using stream metadata $maxCount property
            await _eventStoreClient.SetStreamMetadataAsync(
                streamName,
                StreamState.NoStream,
                new StreamMetadata(1),
                cancellationToken: cancellationToken
            );

            // append event again expecting stream to not exist
            await _eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.NoStream,
                eventToAppend,
                cancellationToken: cancellationToken
            );
        }
    }
    
    private static string GetCheckpointStreamName(string projectionName) => $"checkpoint_{projectionName}";
}