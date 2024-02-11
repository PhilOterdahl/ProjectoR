namespace ProjectoR.Examples.CustomSubscription.Data;

public class EventRecord
{
    public string StreamName { get; set; }
    public Guid Id { get; set; }
    public byte[] Data { get; set; }
    public long Position { get; set; }
    public string EventName { get; set; }
    public DateTimeOffset Created { get; set; }
}