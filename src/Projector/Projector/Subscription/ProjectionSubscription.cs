namespace Projector.Subscription;

public interface IProjectionSubscription
{
    Task Initialize(CancellationToken cancellationToken);
    Task Subscribe(CancellationToken cancellationToken);
    Task UpdateProjections(CancellationToken cancellationToken);
    Task Stop(CancellationToken cancellationToken);
}