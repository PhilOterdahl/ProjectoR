using Humanizer;

namespace ProjectoR.Core.EventNameFormatters;

public class SnakeCaseEventNameFormatter : IEventNameFormatter
{
    public string Format(string eventName)
    {
        return eventName
            .Replace(".", string.Empty)
            .Underscore();
    }
}