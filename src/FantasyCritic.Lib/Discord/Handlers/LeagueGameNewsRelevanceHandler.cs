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

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
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

        return CheckCommonLeagueRelevance(masterGame, currentDate);
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

        //Notable miss logic will override a false condition from the common relevance check if the game meets these conditions
        //If the game is a notable miss, and if the user wants to see it
        bool isNotableMiss = newCriticScore >= NotableMissSetting.Threshold
            && _activeLeagueYears.Any(leagueYear => leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate));

        if (isNotableMiss && CheckNotableMissRelevance(initialScore))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckIsPickedGame(MasterGame masterGame)
    {
        return _activeLeagueYears.Any(leagueYear => leagueYear.Publishers.Any(publisher => publisher.MyMasterGames.Contains(masterGame)));
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

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    private bool CheckSingleLeagueCommonRelevance(LeagueYear leagueYear, MasterGame masterGame, LocalDate currentDate)
    {
        bool eligibleInYear = leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate);
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

        //If ShowPickedGameNews is turned off, and the game is picked in the league year, we don't want to show it
        if (!_gameNewsSetting.ShowAlreadyReleasedNews && masterGame.IsReleased(currentDate))
        {
            return false;
        }

        //If ShowWillReleaseInYearNews is turned off, and the game will release in the league year, we don't want to show it
        if (!_gameNewsSetting.ShowWillReleaseInYearNews && masterGame.WillReleaseInYear(leagueYear.Year))
        {
            return false;
        }

        //If ShowAlreadyReleasedGameNews is turned off, and the game is already released, we don't want to show it
        if (!_gameNewsSetting.ShowMightReleaseInYearNews && masterGame.MightReleaseInYear(leagueYear.Year))
        {
            return false;
        }

        //if ShowAlreadyReleasedGameNews is turned off, and the game is already released, we don't want to show it
        if (!_gameNewsSetting.ShowWillNotReleaseInYearNews && !masterGame.CouldReleaseInYear(leagueYear.Year))
        {
            return false;
        }

        //Fallback is true because all the checks above are filters, if it passes all of them then the news is relevant
        //If something is still showing up that we don't want to see we are missing a filter condition above
        return true;
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
