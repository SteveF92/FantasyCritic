
namespace FantasyCritic.Lib.Discord.GameNewsSettings;
public class OnDiscordGameNewsSetting : DiscordGameNewsSetting
{
    public override string Name => "On";

    public override bool NewGameIsRelevant(MasterGame masterGame, int year) => true;
    public override bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel) => true;
    public override bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel) => true;
}
