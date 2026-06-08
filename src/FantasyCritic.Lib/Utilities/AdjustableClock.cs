using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Utilities;

/// <summary>
/// An <see cref="IClock"/> whose current time can be controlled programmatically.
/// Intended for integration-test scenarios that need to advance the application's
/// sense of "now" without waiting for real time to pass.
///
/// Time formula: targetAt + (realNow - setAt)
/// This means fake time advances naturally between Set calls, so records written
/// at different moments always get distinct timestamps.
/// </summary>
public sealed class AdjustableClock : IClock
{
    private readonly object _lock = new();
    private Instant _setAt;
    private Instant _targetAt;

    public AdjustableClock()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        _setAt = now;
        _targetAt = now;
    }

    public Instant GetCurrentInstant()
    {
        lock (_lock)
        {
            return _targetAt + (SystemClock.Instance.GetCurrentInstant() - _setAt);
        }
    }

    /// <summary>
    /// Sets the fake current time unconditionally. May go backwards.
    /// Use this to establish the starting point of a test scenario.
    /// </summary>
    public void SetInitialTime(Instant target)
    {
        lock (_lock)
        {
            _setAt = SystemClock.Instance.GetCurrentInstant();
            _targetAt = target;
        }
    }

    /// <summary>
    /// Advances the fake current time. Only accepts targets >= the current fake time.
    /// Use this to move time forward during a test scenario without risk of accidentally
    /// drifting backwards.
    /// </summary>
    public Result SetTime(Instant target)
    {
        lock (_lock)
        {
            // Compare against _targetAt (the explicitly anchored base time), not the
            // continuously-advancing fake time. This prevents going backwards relative
            // to any deliberately-set checkpoint, while avoiding spurious failures
            // caused by real-clock drift between an external GetCurrentInstant() call
            // and the interior of this lock.
            if (target < _targetAt)
            {
                return Result.Failure($"Target time {target} is in the past relative to the current fake time {_targetAt}.");
            }

            _setAt = SystemClock.Instance.GetCurrentInstant();
            _targetAt = target;
            return Result.Success();
        }
    }
}
