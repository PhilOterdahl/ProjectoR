using Humanizer;

namespace ProjectoR.Core.EventNameFormatters;

public class PascalCaseEventNameFormatter : IEventNameFormatter
{
    public string Format(string eventName) =>
        eventName
            .Replace(".", string.Empty)
            .Pascalize();
}