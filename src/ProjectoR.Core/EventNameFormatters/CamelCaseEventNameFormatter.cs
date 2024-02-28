using Humanizer;

namespace ProjectoR.Core.EventNameFormatters;

public class CamelCaseEventNameFormatter : IEventNameFormatter
{
    public string Format(string eventName) =>
        eventName
            .Replace(".", string.Empty)
            .Camelize();
}