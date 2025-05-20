using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Discord.Models;
using Serilog;

public class GameNewsOnlyRelevanceHandler : BaseGameNewsRelevanceHandler
{
    private static readonly ILogger _logger = Log.ForContext<GameNewsOnlyRelevanceHandler>();

    public GameNewsOnlyRelevanceHandler(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting,
        GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags, DiscordChannelKey channelKey)
        : base(showPickedGameNews, showEligibleGameNews, notableMissSetting, gameNewsSetting, skippedTags, channelKey)
    {
    }

    public override bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled new game news for this channel
        if (_gameNewsSetting.ShowNewGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(masterGame,currentDate);
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

        //Fallback to false if no specific relevance is found
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    public override bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate)
    {
        //Exit Early if the user has disabled edited game news for this channel
        if (_gameNewsSetting.ShowEditedGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
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

        //Fallback to false if no specific relevance is found
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    public override bool ReleasedGameIsRelevant(MasterGame masterGame, LocalDate currentDate)
    {
        //Exit Early if the user has disabled released game news for this channel
        if (_gameNewsSetting.ShowReleasedGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
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

        //Fallback to false if no specific relevance is found
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    public override bool ScoredGameIsRelevant(MasterGame masterGame, decimal? oldCriticScore, decimal? newCriticScore, LocalDate currentDate)
    {
        //Exit Early if the user has disabled score game news for this channel
        if (_gameNewsSetting.ShowScoreGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(masterGame, currentDate);
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

        //Fallback to false if no specific relevance is found
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _channelKey);
        return false;
    }

    private bool CheckCommonGameNewsOnlyRelevance(MasterGame masterGame, LocalDate currentDate)
    {
        //If user set any tags to be skipped, check if the game has any of those tags
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        //If user asked for all game news about games that will be released in the year
        //Check if the game will be, and return true if so
        if (_gameNewsSetting.ShowWillReleaseInYearNews)
        {
            if (masterGame.WillReleaseInYear(currentDate.Year))
            {
                return true;
            }
        }

        //If user asked for all game news about games that might be released in the year
        //Check to see if it might be released in the year, and return true if so
        if (_gameNewsSetting.ShowMightReleaseInYearNews)
        {
            if (masterGame.MightReleaseInYear(currentDate.Year))
            {
                return true;
            }
        }

        //If user asked for all game news about games that will not be released in the year
        //Check to see if it will not be released in the year, and return true if so
        if (_gameNewsSetting.ShowWillNotReleaseInYearNews)
        {
            if (!masterGame.WillReleaseInYear(currentDate.Year))
            {
                return true;
            }
        }

        //Fallback
        return false;
    }
}
