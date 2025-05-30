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
}
