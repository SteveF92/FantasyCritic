namespace FantasyCritic.Lib.Configuration;

/// <summary>
/// The environments the site runs in, in order, so that a configuration value can be required from one of them onward.
/// </summary>
public enum FantasyCriticEnvironment
{
    Development,
    Beta,
    Production
}
