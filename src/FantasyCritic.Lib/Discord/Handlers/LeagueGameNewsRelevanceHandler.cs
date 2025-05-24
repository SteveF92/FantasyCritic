using FantasyCritic.Lib.Discord.Models;
using Serilog;

namespace FantasyCritic.Lib.Discord.Handlers;

public class LeagueGameNewsRelevanceHandler : BaseGameNewsRelevanceHandler
{
    private static readonly ILogger Logger = Log.ForContext<LeagueGameNewsRelevanceHandler>();
    private readonly IReadOnlyList<LeagueYear> _activeLeagueYears;

    private readonly bool _showPickedGameNews;
    private readonly bool _showEligibleGameNews;
    private readonly bool _showIneligibleGameNews;
    private readonly NotableMissSetting _notableMissSetting;

    public LeagueGameNewsRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, bool showIneligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey, IReadOnlyList<LeagueYear> activeLeagueYears)
        : base(gameNewsSetting, skippedTags, channelKey)
    {
        _showPickedGameNews = showPickedGameNews;
        _showEligibleGameNews = showEligibleGameNews;
        _showIneligibleGameNews = showIneligibleGameNews;
        _notableMissSetting = notableMissSetting;
        _activeLeagueYears = activeLeagueYears;
    }

    public override bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled new game news for this channel
        if (!_gameNewsSetting.ShowNewGameNews)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance)
        {
            return true;
        }

        return false;
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        //Exit Early if the user has disabled game edit news for this channel
        if (!_gameNewsSetting.ShowEditedGameNews)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance)
        {
            return true;
        }

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    public override bool ReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance)
        {
            return true;
        }

        return _gameNewsSetting.ShowReleasedGameNews;
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate)
    {
        bool initialScore = oldCriticScore == null;

        //Exit Early if the user has disabled score game news for this channel
        if (!_gameNewsSetting.ShowScoreGameNews)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance)
        {
            return true;
        }

        //If the game is a notable miss, and if the user wants to see it
        bool isNotableMiss = newCriticScore >= NotableMissSetting.Threshold;
        if (isNotableMiss && CheckNotableMissRelevance(initialScore))
        {
            return true;
        }

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    private bool CheckCommonLeagueRelevance(MasterGame masterGame, LocalDate currentDate)
    {
        //If all settings are turned on of course the league wants to see the game updates
        if (_gameNewsSetting.IsAllOn()
            && _showPickedGameNews
            && _showEligibleGameNews
            && _showIneligibleGameNews)
        {
            return true;
        }

        //Now check the years in the league and compare the news settings
        bool isRelevantToAnyLeagueYear = _activeLeagueYears.Any(x => CheckSingleLeagueCommonRelevance(x, masterGame, currentDate));
        if (isRelevantToAnyLeagueYear)
        {
            return true;
        }

        //If the game has any skipped tags don't show it!
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    private bool CheckSingleLeagueCommonRelevance(LeagueYear leagueYear, MasterGame masterGame, LocalDate currentDate)
    {
        bool inPublisherRoster = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
        bool eligibleInYear = leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate);

        //This is the logic for only showing picked games in the league
        if (!_showPickedGameNews && inPublisherRoster)
        {
            return false;
        }

        //If the game is in the publisher roster we always want to show it
        if (_showPickedGameNews && inPublisherRoster)
        {
            return true;
        }

        //From now on the master game is not in the publisher roster
        //If Eligible Game News is turned off, and the game is eligible in the league year, we don't want to show it
        if (!_showEligibleGameNews && eligibleInYear)
        {
            return false;
        }

        //If I don't want news for ineligible games, and this game is ineligible, don't show it.
        if (!_showIneligibleGameNews && !eligibleInYear)
        {
            return false;
        }

        //This will provide game news for any game that is slated to release in the leagues years
        if (_gameNewsSetting.ShowWillReleaseInYearNews && masterGame.WillReleaseInYear(leagueYear.Year))
        {
            return true;
        }

        //This will provide game updates for any game that might release in the leagues years
        if (_gameNewsSetting.ShowMightReleaseInYearNews && masterGame.MightReleaseInYear(leagueYear.Year))
        {
            return true;
        }

        //This will provide game updates for any game that will not release in the leagues years
        if (_gameNewsSetting.ShowWillNotReleaseInYearNews && !masterGame.WillReleaseInYear(leagueYear.Year))
        {
            return true;
        }

        return false;
    }

    private bool CheckNotableMissRelevance(bool initialScore)
    {
        if (_notableMissSetting == NotableMissSetting.None)
        {
            return false;
        }
        if (_notableMissSetting == NotableMissSetting.ScoreUpdates)
        {
            return true;
        }
        if (_notableMissSetting == NotableMissSetting.InitialScore)
        {
            return initialScore;
        }

        throw new ArgumentOutOfRangeException(nameof(_notableMissSetting), _notableMissSetting, null);
    }
}
