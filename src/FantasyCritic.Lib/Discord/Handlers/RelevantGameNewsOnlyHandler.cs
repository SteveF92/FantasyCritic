using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Discord.Models;

public class GameNewsOnlyRelevanceHandler : BaseGameNewsRelevanceHandler
{
    public GameNewsOnlyRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey)
        : base(showPickedGameNews, showEligibleGameNews, notableMissSetting, gameNewsSetting, skippedTags, channelKey)
    {
    }

    public override bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public override bool ReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }
}
