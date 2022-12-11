
namespace FantasyCritic.Lib.Discord.GameNewsSettings;
public class AllDiscordGameNewsSetting : DiscordGameNewsSetting
{
    public override string Name => "All";

    public override bool NewGameIsRelevant(MasterGame masterGame, int year) => true;
    public override bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel) => true;
    public override bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel) => true;
}
