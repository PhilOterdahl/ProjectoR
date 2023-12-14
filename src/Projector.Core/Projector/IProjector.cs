namespace Projector.Core.Projector;

public interface IProjector
{
    string ProjectionName { get; }
    Type[] EventTypes { get; }
    string[] EventNames { get; }
    long? Position { get; }
    Task Start(CancellationToken cancellationToken);
    Task Project(IEnumerable<EventData> eventData, CancellationToken cancellationToken);
}