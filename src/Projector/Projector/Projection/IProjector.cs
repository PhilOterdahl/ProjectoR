namespace Projector.Projector;

public interface IProjector
{
    string ProjectionName { get; }
    string[] EventTypes { get; }
    long? Position { get; }
    Task Start(CancellationToken cancellationToken);
    Task Project(IEnumerable<EventRecord> events, CancellationToken cancellationToken);
}