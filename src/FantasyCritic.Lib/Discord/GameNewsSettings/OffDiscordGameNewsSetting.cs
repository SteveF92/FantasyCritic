
namespace FantasyCritic.Lib.Discord.GameNewsSettings;
public class OffDiscordGameNewsSetting : DiscordGameNewsSetting
{
    public override string Name => "Off";

    public override bool NewGameIsRelevant(MasterGame masterGame, int year) => false;
    public override bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel) => false;
    public override bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, MinimalLeagueChannel channel) => false;
}
