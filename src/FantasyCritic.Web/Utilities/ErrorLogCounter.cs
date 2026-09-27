using Serilog.Core;
using Serilog.Events;

namespace FantasyCritic.Web.Utilities;

public record ErrorLogSummary(int Count, Instant? LastErrorAt);

/// <summary>
/// Counts the errors this process has logged since it started, for the admin monitor. A Serilog sink, so it sees
/// everything that reaches the logger, the framework's own errors included.
/// </summary>
public class ErrorLogCounter : ILogEventSink
{
    private readonly Lock _lock = new();
    private int _count;
    private Instant? _lastErrorAt;

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Error)
        {
            return;
        }

        lock (_lock)
        {
            _count++;
            _lastErrorAt = Instant.FromDateTimeOffset(logEvent.Timestamp);
        }
    }

    public ErrorLogSummary GetSummary()
    {
        lock (_lock)
        {
            return new ErrorLogSummary(_count, _lastErrorAt);
        }
    }
}
