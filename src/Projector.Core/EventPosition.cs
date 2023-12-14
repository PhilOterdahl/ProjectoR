namespace Projector.Core;

public record EventPosition
{
    private readonly long _value;

    public static implicit operator long(EventPosition position) => position._value;
    public static implicit operator EventPosition(long position) => new(position);
    
    private EventPosition(long position)
    {
        if (position <= 0)
            throw new ArgumentOutOfRangeException(nameof(position), "Position needs to be greater than zero");

        _value = position;
    }
}