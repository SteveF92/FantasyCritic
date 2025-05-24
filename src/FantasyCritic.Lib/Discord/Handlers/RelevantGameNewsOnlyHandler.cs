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

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        //Exit Early if the user has disabled edited game news for this channel
        if (!_gameNewsSetting.ShowEditedGameNews)
        {
            return false;
        }

        return CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
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

    private bool CheckCommonGameNewsOnlyRelevance(MasterGame masterGame, LocalDate currentDate)
    {
        if (_gameNewsSetting.IsAllOn())
        {
           return true;
        }

        //If user set any tags to be skipped, check if the game has any of those tags
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        //If user has turned off already released game news, and the game is released return false
        if (!_gameNewsSetting.ShowAlreadyReleasedNews && masterGame.IsReleased(currentDate))
        {
            return false;
        }

        //if the user has turned off WillReleaseInYear news, and the game will release in the year, return false
        if (!_gameNewsSetting.ShowWillReleaseInYearNews && masterGame.WillReleaseInYear(currentDate.Year))
        {
            return false;
        }

        //If user has turned off MightReleaseInYear news, and the game might release in the year, return false
        if (!_gameNewsSetting.ShowMightReleaseInYearNews && masterGame.MightReleaseInYear(currentDate.Year))
        {
            return false;
        }

        //if the user has turned off WillNotReleaseInYear news, and the game will not release in the year, return false
        if (!_gameNewsSetting.ShowWillNotReleaseInYearNews && !masterGame.WillReleaseInYear(currentDate.Year))
        {
            return false;
        }

        //Fallback is true because all the checks above are filters, if it passes all of them then the news is relevant
        return true;
    }
}
