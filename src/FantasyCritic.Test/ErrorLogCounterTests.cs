using System;
using FantasyCritic.Web.Utilities;
using NodaTime;
using NUnit.Framework;
using Serilog.Events;

namespace FantasyCritic.Test;

[TestFixture]
public class ErrorLogCounterTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 27, 18, 0, 0, TimeSpan.Zero);

    [Test]
    public void NothingLogged_HasNoErrors()
    {
        Assert.That(new ErrorLogCounter().GetSummary(), Is.EqualTo(new ErrorLogSummary(0, null)));
    }

    [Test]
    public void CountsErrorsAndWorse_AndKeepsTheLastOnesTime()
    {
        var counter = new ErrorLogCounter();
        counter.Emit(CreateEvent(LogEventLevel.Error, Start));
        counter.Emit(CreateEvent(LogEventLevel.Information, Start.AddMinutes(1)));
        counter.Emit(CreateEvent(LogEventLevel.Warning, Start.AddMinutes(2)));
        counter.Emit(CreateEvent(LogEventLevel.Fatal, Start.AddMinutes(3)));
        counter.Emit(CreateEvent(LogEventLevel.Warning, Start.AddMinutes(4)));

        Assert.That(counter.GetSummary(), Is.EqualTo(new ErrorLogSummary(2, Instant.FromDateTimeOffset(Start.AddMinutes(3)))));
    }

    private static LogEvent CreateEvent(LogEventLevel level, DateTimeOffset timestamp)
    {
        return new LogEvent(timestamp, level, null, MessageTemplate.Empty, []);
    }
}
