namespace Projector.Core;

public record EventData(string EventName, byte[] Data, long Position);