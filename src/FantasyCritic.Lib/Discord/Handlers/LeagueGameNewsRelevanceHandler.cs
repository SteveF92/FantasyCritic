using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Handlers;
public class LeagueGameNewsRelevanceHandler : BaseGameNewsRelevanceHandler
{
    private readonly IReadOnlyList<LeagueYear> _activeLeagueYears;

    public LeagueGameNewsRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey, IReadOnlyList<LeagueYear> activeLeagueYears)
        : base(showPickedGameNews, showEligibleGameNews, notableMissSetting, gameNewsSetting, skippedTags, channelKey)
    {
        _activeLeagueYears = activeLeagueYears;
    }

    public override bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public override bool ReleasedGameIsRelevant(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? criticScore, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }
}
