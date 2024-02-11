namespace ProjectoR.Examples.Common.Domain;

public abstract class Aggregate<TState> : IAggregate
    where TState : AggregateState, new()
{
    public bool HasUncommittedEvents() => UncommittedEvents.Any();
    public IEnumerable<object> GetAllEvents() => AllEvents.ToArray();
    public IEnumerable<object> GetUncommittedEvents() => UncommittedEvents.ToArray();

    public long Position => AllEvents.Count;
    public long CommitPosition => Position - UncommittedEvents.Count;
    
    protected TState State { get; private set; } = new();
    
    private List<object> AllEvents { get; set; } = [];
    private List<object> UncommittedEvents { get; } = [];

    public static implicit operator TState(Aggregate<TState> load) => load.State;

    protected abstract TState ApplyEvent(TState currentState, object @event);

    public void Load(object[] events)
    {
        State = events.Aggregate(State, ApplyEvent);
        AllEvents.AddRange(events);
    }
    
    public void ClearUncommittedEvents() =>
        UncommittedEvents.Clear();
    
    protected void AddEvent(object @event)
    {
        State = ApplyEvent(State, @event);
        UncommittedEvents.Add(@event);
        AllEvents.Add(@event);
    }
}