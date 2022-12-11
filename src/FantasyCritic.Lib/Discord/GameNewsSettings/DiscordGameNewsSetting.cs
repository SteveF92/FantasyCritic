
namespace FantasyCritic.Lib.Discord.GameNewsSettings;
public abstract class DiscordGameNewsSetting : IEquatable<DiscordGameNewsSetting>
{
    public static Result<DiscordGameNewsSetting> TryFromName(string name)
    {
        return name switch
        {
            "Off" => new OffDiscordGameNewsSetting(),
            "On" => new OnDiscordGameNewsSetting(),
            "Relevant" => new RelevantDiscordGameNewsSetting(),
            "LeagueGamesOnly" => new LeagueGamesOnlyDiscordGameNewsSetting(),
            _ => Result.Failure<DiscordGameNewsSetting>("No matching setting")
        };
    }

    public static DiscordGameNewsSetting FromName(string name)
    {
        var result = TryFromName(name);
        if (result.IsFailure)
        {
            throw new Exception(result.Error);
        }

        return result.Value;
    }

    public abstract string Name { get; }

    public abstract bool NewGameIsRelevant(MasterGame masterGame, int year);
    public abstract bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel);
    public abstract bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel);

    public override string ToString() => Name;

    public bool Equals(DiscordGameNewsSetting? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DiscordGameNewsSetting) obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
