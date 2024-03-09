namespace ProjectoR.Examples.Common.Domain;

public interface IAggregate
{
    public long Position { get; }
    public long CommitPosition { get; }
    public bool HasUncommittedEvents();
    public IEnumerable<object> GetAllEvents();
    public IEnumerable<object> GetUncommittedEvents();
    public void ClearUncommittedEvents();
    public void Load(object[] events);
}