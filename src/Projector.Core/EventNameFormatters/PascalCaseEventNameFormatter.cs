using Humanizer;

namespace ProjectoR.Core.EventNameFormatters;

public class PascalCaseEventNameFormatter : IEventNameFormatter
{
    public string Format(string eventName)
    {
        return eventName
            .Replace(".", string.Empty)
            .Pascalize();
    }
}