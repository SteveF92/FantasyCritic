using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Handlers;
public abstract class BaseGameNewsRelevanceHandler
{
    protected readonly bool _showPickedGameNews;
    protected readonly bool _showEligibleGameNews;
    protected readonly NotableMissSetting _notableMissSetting;
    protected readonly GameNewsSetting _gameNewsSetting;
    protected readonly IReadOnlyList<MasterGameTag> _skippedTags;
    protected readonly DiscordChannelKey _channelKey;

    protected BaseGameNewsRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey)
    {
        _showPickedGameNews = showPickedGameNews;
        _showEligibleGameNews = showEligibleGameNews;
        _notableMissSetting = notableMissSetting;
        _gameNewsSetting = gameNewsSetting;
        _skippedTags = skippedTags;
        _channelKey = channelKey;
    }

    public abstract bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate);
    public abstract bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate);
    public abstract bool ReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate);
    public abstract bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate);
}
