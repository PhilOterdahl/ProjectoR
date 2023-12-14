namespace Projector.Core.Projector;

public abstract class BatchProjector<TConnection>(TConnection connection, IServiceProvider serviceProvider) : BaseProjector<TConnection>(connection, serviceProvider)
{
    protected abstract Task<long> ProjectBatch(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken = default);
    
    protected override async Task Project(TConnection connection, IEnumerable<EventRecord> events, CancellationToken cancellationToken)
    {
        var lastPosition = await ProjectBatch(connection, events, cancellationToken).ConfigureAwait(false);
        await SaveCheckpoint(lastPosition, cancellationToken).ConfigureAwait(false);
    }
}