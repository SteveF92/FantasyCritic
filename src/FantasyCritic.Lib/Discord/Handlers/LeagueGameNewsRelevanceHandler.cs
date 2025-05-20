using FantasyCritic.Lib.Discord.Models;
using Serilog;

namespace FantasyCritic.Lib.Discord.Handlers;

public class LeagueGameNewsRelevanceHandler : BaseGameNewsRelevanceHandler
{
    private static readonly ILogger Logger = Log.ForContext<LeagueGameNewsRelevanceHandler>();
    private readonly IReadOnlyList<LeagueYear> _activeLeagueYears;

    public LeagueGameNewsRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey, IReadOnlyList<LeagueYear> activeLeagueYears)
        : base(showPickedGameNews, showEligibleGameNews, notableMissSetting, gameNewsSetting, skippedTags, channelKey)
    {
        _activeLeagueYears = activeLeagueYears;
    }

    public override bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled new game news for this channel
        if (_gameNewsSetting.ShowNewGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance == true)
        {
            return true;
        }

        //Specific New Game Relevance Logic
        bool specificRelevance = false;

        return specificRelevance;
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        //Exit Early if the user has disabled game edit news for this channel
        if (_gameNewsSetting.ShowEditedGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance == true)
        {
            return true;
        }

        //Specific Edited Game Relevance Logic
        bool specificRelevance = false;

        if (specificRelevance == true)
        {
            return true;
        }

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    public override bool ReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled Released game news for this channel
        if (_gameNewsSetting.ShowReleasedGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance == true)
        {
            return true;
        }

        //Specific Edited Game Relevance Logic
        bool specificRelevance = false;

        if (specificRelevance == true)
        {
            return true;
        }

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate)
    {
        bool initialScore = oldCriticScore == null;

        //Exit Early if the user has disabled score game news for this channel
        if (_gameNewsSetting.ShowScoreGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonLeagueRelevance(masterGame, currentDate);
        if (commonRelevance == true)
        {
            return true;
        }

        //Specific Score News Relevance Logic
        bool specificRelevance = false;

        bool isNotableMiss = newCriticScore >= NotableMissSetting.Threshold;

        //If the game is a notable miss, check if the user wants to see it
        if (isNotableMiss)
        {
            specificRelevance = CheckNotableMissRelevance(initialScore);
        }

        if (specificRelevance == true)
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
            && _showEligibleGameNews)
        {
            return true;
        }

        //If the game has any skipped tags dont show it!
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        //Now check the years in the league and compare the news settings
        foreach (var leagueYear in _activeLeagueYears)
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
        }

        //Fallback
        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
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
