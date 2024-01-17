namespace ProjectoR.Examples.CustomSubscription;

public class EventRecord
{
    public Guid Id { get; set; }
    public byte[] Data { get; set; }
    public long Position { get; set; }
    public string Name { get; set; }
}