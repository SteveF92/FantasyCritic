using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Handlers;
public abstract class BaseGameNewsRelevanceHandler
{
    protected readonly GameNewsSetting _gameNewsSetting;
    protected readonly IReadOnlyList<MasterGameTag> _skippedTags;
    protected readonly DiscordChannelKey _channelKey;

    protected BaseGameNewsRelevanceHandler(GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey)
    {
        _gameNewsSetting = gameNewsSetting;
        _skippedTags = skippedTags;
        _channelKey = channelKey;
    }

    public abstract bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate);
    public abstract bool ExistingGameIsRelevant(MasterGame masterGame, WillReleaseStatus? prevReleaseStatus, LocalDate currentDate);
    public abstract bool JustReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate);
    public abstract bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate);
    protected bool IsPrevReleaseStatusOverride(WillReleaseStatus? prevReleaseStatus)
    {

        if (prevReleaseStatus == WillReleaseStatus.WillRelease && _gameNewsSetting.ShowWillReleaseInYearNews)
        {
            return true;
        }

        if (prevReleaseStatus == WillReleaseStatus.MightRelease && _gameNewsSetting.ShowMightReleaseInYearNews)
        {
            return true;
        }

        if (prevReleaseStatus == WillReleaseStatus.WillNotRelease && _gameNewsSetting.ShowWillNotReleaseInYearNews)
        {
            return true;
        }

        return false;
    }

    protected bool IsReleaseStatusRelevant(MasterGame masterGame, WillReleaseStatus? prevReleaseStatus, LocalDate currentDate)
    {
        //If user has turned off already released game news, and the game is released return false unless the previous release status is overridden
        if (!_gameNewsSetting.ShowAlreadyReleasedNews
            && masterGame.IsReleased(currentDate)
            && !IsPrevReleaseStatusOverride(prevReleaseStatus))
        {
            return false;
        }

        //if the user has turned off WillReleaseInYear news, and the game will release in the year, return false unless the previous release status is overridden
        if (!_gameNewsSetting.ShowWillReleaseInYearNews
            && masterGame.WillReleaseInYear(currentDate.Year) && !masterGame.IsReleased(currentDate)
            && !IsPrevReleaseStatusOverride(prevReleaseStatus))
        {
            return false;
        }

        //If user has turned off MightReleaseInYear news, and the game might release in the year, return false unless the previous release status is overridden
        if (!_gameNewsSetting.ShowMightReleaseInYearNews
            && masterGame.MightReleaseInYear(currentDate.Year)
            && !IsPrevReleaseStatusOverride(prevReleaseStatus))
        {
            return false;
        }

        //if the user has turned off WillNotReleaseInYear news, and the game will not release in the year, return false unless the previous release status is overridden
        if (!_gameNewsSetting.ShowWillNotReleaseInYearNews
            && !masterGame.CouldReleaseInYear(currentDate.Year)
            && !IsPrevReleaseStatusOverride(prevReleaseStatus))
        {
            return false;
        }

        return true;
    }
}
