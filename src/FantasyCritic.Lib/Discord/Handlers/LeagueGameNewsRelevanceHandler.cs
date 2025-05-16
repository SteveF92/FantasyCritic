using FantasyCritic.Lib.Discord.Interfaces;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Handlers;
internal class LeagueGameNewsRelevanceHandler : IGameNewsRelevanceHandler
{
    private readonly bool _showPickedGameNews;
    private readonly bool _showEligibleGameNews;
    private readonly NotableMissSetting _notableMissSetting;
    private readonly GameNewsSetting _gameNewsSetting;
    private readonly IReadOnlyList<MasterGameTag> _skippedTags;
    private readonly IReadOnlyList<LeagueYear> _activeLeagueYears;
    private readonly DiscordChannelKey _channelKey;

    public LeagueGameNewsRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, IReadOnlyList<LeagueYear> activeLeagueYears, DiscordChannelKey channelKey)
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
        throw new NotImplementedException();
    }

    public bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public bool ReleasedGameIsRelevant(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public bool ScoredGameIsRelevant(MasterGame masterGame, decimal? criticScore, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }
}
