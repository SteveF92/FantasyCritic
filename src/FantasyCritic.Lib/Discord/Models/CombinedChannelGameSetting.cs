using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Discord.Interfaces;

namespace FantasyCritic.Lib.Discord.Models;
public class CombinedChannelGameSetting
{
    private readonly bool _showPickedGameNews;
    private readonly bool _showEligibleGameNews;
    private readonly NotableMissSetting _notableMissSetting;
    private readonly GameNewsSetting _gameNewsSetting;
    private readonly IReadOnlyList<MasterGameTag> _skippedTags;
    private readonly IReadOnlyList<LeagueYear>? _activeLeagueYears;
    private readonly DiscordChannelKey _channelKey;

    public CombinedChannelGameSetting(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting, GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags,
        IReadOnlyList<LeagueYear>? activeLeagueYears, DiscordChannelKey channelKey)
    {
        _showPickedGameNews = showPickedGameNews;
        _showEligibleGameNews = showEligibleGameNews;
        _notableMissSetting = notableMissSetting;
        _gameNewsSetting = gameNewsSetting;
        _skippedTags = skippedTags;
        _activeLeagueYears = activeLeagueYears;
        _channelKey = channelKey;
    }

    public bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        return GetRelevanceHandler().NewGameIsRelevant(masterGame, currentDate);
    }

    public bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        return GetRelevanceHandler().ExistingGameIsRelevant(masterGame, releaseStatusChanged, currentDate);
    }

    public bool ReleasedGameIsRelevant(MasterGame masterGame)
    {
        return GetRelevanceHandler().ReleasedGameIsRelevant(masterGame);
    }

    public bool ScoredGameIsRelevant(MasterGame masterGame, decimal? criticScore, LocalDate currentDate)
    {
        return GetRelevanceHandler().ScoredGameIsRelevant(masterGame, criticScore, currentDate);
    }

    private IGameNewsRelevanceHandler GetRelevanceHandler()
    {
        if (_activeLeagueYears is not null)
        {
            return new LeagueGameNewsRelevanceHandler(_showPickedGameNews, _showEligibleGameNews, _notableMissSetting, _gameNewsSetting, _skippedTags, _activeLeagueYears, _channelKey);
        }

        return new GameNewsOnlyRelevanceHandler(_showPickedGameNews, _showEligibleGameNews, _notableMissSetting, _gameNewsSetting, _skippedTags, _channelKey);
    }
}
