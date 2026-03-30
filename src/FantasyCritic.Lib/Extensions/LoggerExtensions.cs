using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Extensions;

public static partial class LoggerExtensions
{
    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex TemplatePlaceholderRegex();

    public static void LogTraceWithContext(this ILogger logger, string messageTemplate, object? additionalFields = null)
    {
        LogWithContext(logger, LogLevel.Trace, messageTemplate, additionalFields, null);
    }

    public static void LogDebugWithContext(this ILogger logger, string messageTemplate, object? additionalFields = null)
    {
        LogWithContext(logger, LogLevel.Debug, messageTemplate, additionalFields, null);
    }

    public static void LogInfoWithContext(this ILogger logger, string messageTemplate, object? additionalFields = null)
    {
        LogWithContext(logger, LogLevel.Information, messageTemplate, additionalFields, null);
    }

    public static void LogWarnWithContext(this ILogger logger, string messageTemplate, object? additionalFields = null)
    {
        LogWithContext(logger, LogLevel.Warning, messageTemplate, additionalFields, null);
    }

    public static void LogErrorWithContext(this ILogger logger, string messageTemplate, object? additionalFields = null, Exception? exception = null)
    {
        LogWithContext(logger, LogLevel.Error, messageTemplate, additionalFields, exception);
    }

    public static void LogCriticalWithContext(this ILogger logger, string messageTemplate, object? additionalFields = null, Exception? exception = null)
    {
        LogWithContext(logger, LogLevel.Critical, messageTemplate, additionalFields, exception);
    }

    private static void LogWithContext(ILogger logger, LogLevel level, string messageTemplate, object? additionalFields, Exception? exception)
    {
        if (!logger.IsEnabled(level))
        {
            return;
        }

        if (additionalFields is null)
        {
            logger.Log(level, exception, messageTemplate);
            return;
        }

        var allProperties = additionalFields.GetType().GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(additionalFields));

        var placeholderNames = TemplatePlaceholderRegex()
            .Matches(messageTemplate)
            .Select(m => m.Groups[1].Value)
            .ToList();

        var templateArgs = placeholderNames
            .Where(allProperties.ContainsKey)
            .Select(name => allProperties[name])
            .ToArray();

        var contextFields = allProperties
            .Where(kvp => !placeholderNames.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (contextFields.Count > 0)
        {
            using (logger.BeginScope(contextFields)!)
            {
                logger.Log(level, exception, messageTemplate, templateArgs);
            }
        }
        else
        {
            logger.Log(level, exception, messageTemplate, templateArgs);
        }
    }
}
