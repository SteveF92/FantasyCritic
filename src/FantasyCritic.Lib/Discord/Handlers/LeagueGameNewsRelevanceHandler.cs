using FantasyCritic.Lib.BusinessLogicFunctions;
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
        if (!_gameNewsSetting.ShowNewGameAnnouncements)
        {
            return false;
        }

        //Common Relevance Logic
        return CheckCommonLeagueRelevance(masterGame, currentDate);
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, WillReleaseStatus? prevReleaseStatus, LocalDate currentDate)
    {
        //Picked Games, Aka games in the publisher roster for any league year will exit early if enable picked game override is true.
        bool isPickedGame = CheckIsPickedGame(masterGame);
        if (isPickedGame && _showPickedGameNews)
        {
            return true;
        }

        //Exit Early if the user has disabled edited game news for this channel
        if (!_gameNewsSetting.ShowEditedGameNews)
        {
            return false;
        }

        return CheckCommonLeagueRelevance(masterGame, currentDate, prevReleaseStatus);
    }

    public override bool JustReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Picked Games, Aka games in the publisher roster for any league year will exit early if enable picked game override is true.
        bool isPickedGame = CheckIsPickedGame(masterGame);
        if (isPickedGame && _showPickedGameNews)
        {
            return true;
        }

        //Exit Early if the user has disabled just released game announcements for this channel
        if (!_gameNewsSetting.ShowJustReleasedAnnouncements)
        {
            return false;
        }

        return CheckCommonLeagueRelevance(masterGame, currentDate);
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate)
    {
        bool initialScore = oldCriticScore == null;

        //Picked Games, Aka games in the publisher roster for any league year will exit early if enable picked game override is true.
        bool isPickedGame = CheckIsPickedGame(masterGame);
        if (isPickedGame && _showPickedGameNews)
        {
            return true;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance && _gameNewsSetting.ShowScoreGameNews)
        {
            return true;
        }

        //Notable miss logic will override a false condition from the common relevance check if the game meets these conditions
        //If the game is a notable miss, and if the user wants to see it
        bool isNotableMiss = newCriticScore >= NotableMissSetting.Threshold
            && _activeLeagueYears.Any(leagueYear => leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate));

        if (isNotableMiss && CheckNotableMissRelevance(initialScore))
        {
            return true;
        }

        return false;
    }

    private bool CheckIsPickedGame(MasterGame masterGame)
    {
        return _activeLeagueYears.Any(leagueYear => leagueYear.Publishers.Any(publisher => publisher.MyMasterGames.Contains(masterGame)));
    }

    private bool CheckCommonLeagueRelevance(MasterGame masterGame, LocalDate currentDate, WillReleaseStatus? prevReleaseStatus = null)
    {
        //If all settings are turned on of course the league wants to see the game updates
        if (_gameNewsSetting.IsAllOn()
            && _showEligibleGameNews
            && _showIneligibleGameNews
            && !_skippedTags.Any())
        {
            return true;
        }

        //Now check the years in the league and compare the news settings
        bool isRelevantToAnyLeagueYear = _activeLeagueYears.Any(x => CheckSingleYearLeagueCommonRelevance(x, masterGame, currentDate, prevReleaseStatus));
        if (isRelevantToAnyLeagueYear)
        {
            return true;
        }

        return false;
    }

    private bool CheckSingleYearLeagueCommonRelevance(LeagueYear leagueYear, MasterGame masterGame, LocalDate currentDate, WillReleaseStatus? prevReleaseStatus)
    {
        bool eligibleInASlot = leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate);
        bool eligibleInYear = eligibleInASlot && !masterGame.CriticScore.HasValue;
        bool ineligibleInYear = !eligibleInYear;

        //If the game has any skipped tags don't show it!
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        //If Eligible Game News is turned off, and the game is eligible in the league year, we don't want to show it
        if (!_showEligibleGameNews && eligibleInYear)
        {
            return false;
        }

        //If  ineligible game news is turned off, and the game is ineligible in the league year, we don't want to show it
        if (!_showIneligibleGameNews && ineligibleInYear)
        {
            return false;
        }

        //If the game is not relevant based on its release status, return false
        if (!IsReleaseStatusRelevant(masterGame, prevReleaseStatus, currentDate))
        {
            return false;
        }

        //Fallback is true because all the checks above are filters, if it passes all of them then the news is relevant
        //If something is still showing up that we don't want to see we are missing a filter condition above
        return true;
    }

    private bool CheckNotableMissRelevance(bool initialScore)
    {
        
        if (_notableMissSetting == NotableMissSetting.ScoreUpdates)
        {
            return true;
        }
        if (_notableMissSetting == NotableMissSetting.InitialScore && initialScore)
        {
            return true;
        }

        return false;
    }
}
