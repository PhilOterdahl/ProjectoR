namespace Projector.Core.Projector;

public interface IProjector
{
    string ProjectionName { get; }
    Type[] EventTypes { get; }
    long? Position { get; }
    Task Start(CancellationToken cancellationToken);
    Task Project(IEnumerable<EventRecord> events, CancellationToken cancellationToken);
}