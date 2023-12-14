using Microsoft.Extensions.DependencyInjection;
using Projector.Core.Checkpointing;

namespace Projector.Core.Projector;

public abstract class BatchProjector<TConnection> : BaseProjector<TConnection>
{
    protected BatchProjector(TConnection connection, ICheckpointRepository checkpointRepository, IKeyedServiceProvider serviceProvider) : base(connection, checkpointRepository, serviceProvider)
    {
    }

    protected abstract Task<long> ProjectBatch(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken = default);
    
    protected override async Task Project(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken)
    {
        var lastPosition = await ProjectBatch(connection, events, cancellationToken);
        await SaveCheckpoint(lastPosition, cancellationToken);
    }
}