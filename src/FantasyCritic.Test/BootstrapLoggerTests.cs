using System;
using System.Collections.Generic;
using FantasyCritic.Hosting;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;

namespace FantasyCritic.Test;

/// <summary>
/// Every host starts with a bootstrap logger and replaces it once its configuration is loaded. These pin what that
/// replacement has to get right. They swap the global logger, so each one puts the suite's own back.
/// </summary>
[TestFixture]
public class BootstrapLoggerTests
{
    private ILogger _suiteLogger = null!;

    [SetUp]
    public void SaveSuiteLogger()
    {
        _suiteLogger = Log.Logger;
    }

    [TearDown]
    public void RestoreSuiteLogger()
    {
        if (!ReferenceEquals(Log.Logger, _suiteLogger))
        {
            (Log.Logger as IDisposable)?.Dispose();
        }

        Log.Logger = _suiteLogger;
    }

    [Test]
    public void LoggerTakenBeforeTheReplacement_FollowsItToTheReplacement()
    {
        var bootstrapSink = new RecordingSink();
        var replacementSink = new RecordingSink();
        FantasyCriticLogging.UseBootstrapLogger(new LoggerConfiguration().WriteTo.Sink(bootstrapSink));

        // Taken the way a static Log.ForContext field is, before the host has loaded its configuration.
        var captured = Log.ForContext<BootstrapLoggerTests>();
        captured.Information("Before");
        FantasyCriticLogging.ReplaceBootstrapLogger(() => new LoggerConfiguration().WriteTo.Sink(replacementSink));
        captured.Information("After");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bootstrapSink.Messages, Is.EqualTo(new[] { "Before" }));
            Assert.That(replacementSink.Messages, Is.EqualTo(new[] { "After" }));
            Assert.That(Log.Logger, Is.Not.InstanceOf<ReloadableLogger>(), "the replacement is frozen, so logging no longer goes through the reload wrapper");
        }
    }

    [Test]
    public void BootstrapLogger_IsClosedBeforeTheReplacementIsBuilt()
    {
        var bootstrapSink = new RecordingSink();
        FantasyCriticLogging.UseBootstrapLogger(new LoggerConfiguration().WriteTo.Sink(bootstrapSink));
        bool? closedWhenReplacementBuilt = null;

        FantasyCriticLogging.ReplaceBootstrapLogger(() =>
        {
            closedWhenReplacementBuilt = bootstrapSink.Disposed;
            return new LoggerConfiguration();
        });

        // A file sink still open here would make the replacement start new log files beside the old ones.
        Assert.That(closedWhenReplacementBuilt, Is.True);
    }

    [Test]
    public void ReplacingALoggerThatIsNotTheBootstrapLogger_Fails()
    {
        Log.Logger = new LoggerConfiguration().CreateLogger();

        Assert.Throws<InvalidOperationException>(() => FantasyCriticLogging.ReplaceBootstrapLogger(() => new LoggerConfiguration()));
    }

    private sealed class RecordingSink : ILogEventSink, IDisposable
    {
        public List<string> Messages { get; } = [];
        public bool Disposed { get; private set; }

        public void Emit(LogEvent logEvent)
        {
            Messages.Add(logEvent.RenderMessage());
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
