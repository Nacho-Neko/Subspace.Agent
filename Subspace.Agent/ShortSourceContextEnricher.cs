using Serilog.Core;
using Serilog.Events;

internal class ShortSourceContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextProperty)
            && sourceContextProperty is ScalarValue scalarValue
            && scalarValue.Value is string fullSourceContext)
        {
            var shortSourceContext = fullSourceContext.Substring(fullSourceContext.LastIndexOf('.') + 1);
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", shortSourceContext));
        }
    }
}