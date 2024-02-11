using System.Text.Json;
using EventStore.Client;
using Humanizer;
using ProjectoR.Core.Checkpointing;

namespace ProjectoR.EventStore.Checkpointing;

internal class EventStoreCheckpointRepository(EventStoreClient eventStoreClient) : ICheckpointRepository
{
    public async Task<Checkpoint?> TryLoad(string projectionName, CancellationToken cancellationToken = default)
    {
        var streamName = GetCheckpointStreamName(projectionName);
        
        var result = eventStoreClient.ReadStreamAsync(
            Direction.Backwards, 
            streamName,
            StreamPosition.End,
            1,
            cancellationToken: cancellationToken
        );
        
        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        ResolvedEvent? @event = await result
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var state = JsonSerializer.Deserialize<CheckpointState>(@event.Value.Event.Data.ToArray());
        return Checkpoint.FromState(state);
    }

    public async Task<Checkpoint> Load(string projectionName, CancellationToken cancellationToken = default) =>
        await TryLoad(projectionName, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException($"Checkpoint not found for projection: {projectionName}");

    public async Task MakeCheckpoint(Checkpoint checkpoint, CancellationToken cancellationToken = default)
    {
        var @event = new EventData(
            Uuid.NewUuid(),
            nameof(Checkpoint).ToLower(),
            JsonSerializer.SerializeToUtf8Bytes((CheckpointState)checkpoint)
        );
        
        var eventToAppend = new[] { @event };
        var streamName = GetCheckpointStreamName(checkpoint.ProjectionName);

        try
        {
            
            // store new checkpoint expecting stream to exist
            await eventStoreClient
                .AppendToStreamAsync(
                    streamName,
                    StreamState.NoStream,
                    eventToAppend,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (WrongExpectedVersionException)
        {
            // WrongExpectedVersionException means that stream did not exist
            // Set the checkpoint stream to have at most 1 event
            // using stream metadata $maxCount property
            await eventStoreClient
                .SetStreamMetadataAsync(
                    streamName,
                    StreamState.NoStream,
                    new StreamMetadata(1),
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        
            // append event again expecting stream to not exist
            await eventStoreClient
                .AppendToStreamAsync(
                    streamName,
                    StreamState.NoStream,
                    eventToAppend,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var test = e;
        }
        finally
        {
            var test = eventStoreClient;
        }

        var tes2 = "test";
    }
    
    private static string GetCheckpointStreamName(string projectionName) => $"checkpoint_{projectionName.Underscore()}";
}