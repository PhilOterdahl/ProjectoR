using Humanizer;

namespace ProjectoR.Core.EventNameFormatters;

public class KebabCaseEventNameFormatter : IEventNameFormatter
{
    public string Format(string eventName)
    {
        return eventName
            .Replace(".", string.Empty)
            .Kebaberize();
    }
}