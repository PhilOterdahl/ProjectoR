using ProjectoR.Core.Projector;

namespace ProjectoR.Core.Subscription;

internal abstract class Subscription(IProjectorService projectorService) : IProjectionSubscription
{
    protected bool Stopping { get; private set; }
    protected IProjectorService ProjectorService { get; } = projectorService;

    public async Task Initialize(CancellationToken cancellationToken) =>
        await ProjectorService
            .Start(cancellationToken)
            .ConfigureAwait(false);

    public abstract Task Subscribe(CancellationToken cancellationToken);

    public async Task UpdateProjections(CancellationToken cancellationToken)  => 
        await ProjectorService
            .UpdateProjections(cancellationToken)
            .ConfigureAwait(false);

    public virtual async Task Stop(CancellationToken cancellationToken)
    {
        Stopping = true;
        await ProjectorService
            .Stop(cancellationToken)
            .ConfigureAwait(false);
    }
}