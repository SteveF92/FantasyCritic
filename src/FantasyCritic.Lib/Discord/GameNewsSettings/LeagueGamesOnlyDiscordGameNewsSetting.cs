
namespace FantasyCritic.Lib.Discord.GameNewsSettings;

public class LeagueGamesOnlyDiscordGameNewsSetting : DiscordGameNewsSetting
{
    public override string Name => "LeagueGamesOnly";

    public override bool NewGameIsRelevant(MasterGame masterGame, int year)
    {
        return false;
    }

    public override bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel)
    {
        return leaguesWithGame.Contains(channel.LeagueID);
    }

    public override bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel)
    {
        return leaguesWithGame.Contains(channel.LeagueID);
    }
}
