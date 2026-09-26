namespace FantasyCritic.Lib.Configuration;

/// <summary>
/// One section of a host's options. It reports the configuration paths under it that the host cannot start without.
/// </summary>
public interface IOptionsSection
{
    IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment);
}

/// <summary>
/// Everything one host reads from configuration: its appsettings.json is an instance of this.
/// </summary>
public interface IHostOptions
{
    /// <summary>
    /// Every host logs to Loki, and builds that logger from its options before validating them.
    /// </summary>
    GrafanaOptions Grafana { get; }

    Result Validate(FantasyCriticEnvironment environment);
}

/// <summary>
/// Collects the configuration paths a host cannot start without, so that a host missing several reports them all at once.
/// </summary>
public sealed class MissingConfiguration
{
    /// <summary>
    /// What appsettings ships in place of a value that belongs in the secret store.
    /// </summary>
    public const string Placeholder = "secret";

    private readonly FantasyCriticEnvironment _environment;
    private readonly List<string> _missing = [];

    public MissingConfiguration(FantasyCriticEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<string> Paths => _missing;

    /// <summary>
    /// A value that must be set. From <paramref name="requiredFrom"/> onward it also must not be the placeholder, so a key
    /// required from Beta may be the placeholder on a developer's machine and nowhere else.
    /// </summary>
    public MissingConfiguration Value(string path, string? value, FantasyCriticEnvironment requiredFrom = FantasyCriticEnvironment.Development)
    {
        if (string.IsNullOrWhiteSpace(value) || (IsPlaceholder(value) && _environment >= requiredFrom))
        {
            _missing.Add(path);
        }

        return this;
    }

    /// <summary>
    /// A value that only has to be bound. Empty is a legitimate setting for it, as it is for the Loki keys, which turn the sink off.
    /// </summary>
    public MissingConfiguration Present(string path, object? value)
    {
        if (value is null)
        {
            _missing.Add(path);
        }

        return this;
    }

    /// <summary>
    /// A nested section. One that did not bind at all is reported by its own path, not once for every key under it.
    /// </summary>
    public MissingConfiguration Section(string path, IOptionsSection? section)
    {
        if (section is null)
        {
            _missing.Add(path);
        }
        else
        {
            _missing.AddRange(section.Validate(path, _environment));
        }

        return this;
    }

    public Result ToResult()
    {
        if (_missing.Count > 0)
        {
            return Result.Failure($"Missing configuration: {string.Join(", ", _missing)}");
        }

        return Result.Success();
    }

    /// <summary>
    /// Whether a value is the placeholder, in any case. Also how Discord push knows to stay off.
    /// </summary>
    public static bool IsPlaceholder(string? value) => string.Equals(value, Placeholder, StringComparison.OrdinalIgnoreCase);
}
