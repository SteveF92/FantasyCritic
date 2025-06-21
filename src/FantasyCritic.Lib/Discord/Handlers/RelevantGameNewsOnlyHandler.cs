using FantasyCritic.Lib.Discord.Models;
using Serilog;

namespace FantasyCritic.Lib.Discord.Handlers;

public class GameNewsOnlyRelevanceHandler : BaseGameNewsRelevanceHandler
{
    private static readonly ILogger _logger = Log.ForContext<GameNewsOnlyRelevanceHandler>();

    public GameNewsOnlyRelevanceHandler(GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey)
        : base(gameNewsSetting, skippedTags, channelKey)
    {
    }

    public override bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled new game news for this channel
        if (!_gameNewsSetting.ShowNewGameAnnouncements)
        {
            return false;
        }

        return CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, WillReleaseStatus? prevReleaseStatus, LocalDate currentDate)
    {
        //Exit Early if the user has disabled edited game news for this channel
        if (!_gameNewsSetting.ShowEditedGameNews)
        {
            return false;
        }

        return CheckCommonGameNewsOnlyRelevance(masterGame, currentDate, prevReleaseStatus);
    }

    public override bool JustReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled released game news for this channel
        if (!_gameNewsSetting.ShowJustReleasedAnnouncements)
        {
            return false;
        }

        return CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate)
    {
        //Exit Early if the user has disabled score game news for this channel
        if (!_gameNewsSetting.ShowScoreGameNews)
        {
            return false;
        }

        return CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
    }

    private bool CheckCommonGameNewsOnlyRelevance(MasterGame masterGame, LocalDate currentDate, WillReleaseStatus? prevReleaseStatus = null)
    {
        if (_gameNewsSetting.IsAllOn()
            && !_skippedTags.Any())
        {
           return true;
        }

        //If user set any tags to be skipped, check if the game has any of those tags
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        //If the game is not relevant based on its release status, return false
        if (!IsReleaseStatusRelevant(masterGame, prevReleaseStatus, currentDate))
        {
            return false;
        }

        //Fallback is true because all the checks above are filters, if it passes all of them then the news is relevant
        return true;
    }
}
