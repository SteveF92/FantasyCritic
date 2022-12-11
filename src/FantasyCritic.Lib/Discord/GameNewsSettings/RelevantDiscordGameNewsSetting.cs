
namespace FantasyCritic.Lib.Discord.GameNewsSettings;
public class RelevantDiscordGameNewsSetting : DiscordGameNewsSetting
{
    public override string Name => "Relevant";

    public override bool NewGameIsRelevant(MasterGame masterGame, int year)
    {
        return masterGame.CouldReleaseInYear(year);
    }

    public override bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel)
    {
        return releaseStatusChanged || leaguesWithGame.Contains(channel.LeagueID) || masterGameYear.IsRelevantInYear(false);
    }

    public override bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel)
    {
        return leaguesWithGame.Contains(channel.LeagueID);
    }
}
