using FantasyCritic.Lib.Discord.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using Serilog;


namespace FantasyCritic.Lib.Discord.Handlers;
public class RelevantGameNewsOnlyHandler : IRelevantGameNewsHandler
{
    private static readonly ILogger _logger = Log.ForContext<RelevantGameNewsOnlyHandler>();
    private GameNewsSettingsRecord _gameNewsSettings;
    private DiscordChannelKey _discordChannelKey;
    public RelevantGameNewsOnlyHandler(GameNewsSettingsRecord gameNewsSettings, DiscordChannelKey discordChannelKey)
    {
        _gameNewsSettings = gameNewsSettings;
        _discordChannelKey = discordChannelKey;
    }
    public bool IsEditedGameNewsRelevant(EditedGameNewsRecord newsRecord)
    {
        MasterGame masterGame = newsRecord.MasterGame;
        LocalDate currentDate = newsRecord.CurrentDate;

        //Exit Early if the user has disabled edited game news for this channel
        if (_gameNewsSettings.ShowEditedGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(newsRecord);
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
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _discordChannelKey);
        return false;
    }

    public bool IsNewGameNewsRelevant(NewGameNewsRecord newsRecord)
    {
        MasterGame masterGame = newsRecord.MasterGame;
        LocalDate currentDate = newsRecord.CurrentDate;

        //Exit Early if the user has disabled new game news for this channel
        if (_gameNewsSettings.ShowNewGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(newsRecord);
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
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _discordChannelKey);
        return false;
    }

    public bool IsReleasedGameNewsRelevant(ReleaseGameNewsRecord newsRecord)
    {
        MasterGame masterGame = newsRecord.MasterGame;
        LocalDate currentDate = newsRecord.CurrentDate;

        //Exit Early if the user has disabled released game news for this channel
        if (_gameNewsSettings.ShowReleasedGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(newsRecord);
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
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _discordChannelKey);
        return false;
    }

    public bool IsScoreGameNewsRelevant(ScoreGameNewsRecord newsRecord)
    {
        MasterGame masterGame = newsRecord.MasterGame;
        LocalDate currentDate = newsRecord.CurrentDate;

        //Exit Early if the user has disabled score game news for this channel
        if (_gameNewsSettings.ShowScoreGameNews == false)
        {
            return false;
        }

        //Common Relevance Logic
        bool commonRelevance = CheckCommonGameNewsOnlyRelevance(newsRecord);
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
        _logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, _discordChannelKey);
        return false;
    }

    private bool CheckCommonGameNewsOnlyRelevance(IGameNewsRecord gameNews)
    {
        MasterGame masterGame = gameNews.MasterGame;
        LocalDate currentDate = gameNews.CurrentDate;

        //If user set any tags to be skipped, check if the game has any of those tags
        if (masterGame.Tags.Intersect(_gameNewsSettings.SkippedTags).Any())
        {
            return false;
        }

        //If user asked for all game news about games that will be released in the year
        //Check if the game will be, and return true if so
        if (_gameNewsSettings.ShowWillReleaseInYearNews)
        {
            if(masterGame.WillReleaseInYear(currentDate.Year))
            {
                return true;
            }
        }

        //If user asked for all game news about games that might be released in the year
        //Check to see if it might be released in the year, and return true if so
        if (_gameNewsSettings.ShowMightReleaseInYearNews)
        {
            if(masterGame.MightReleaseInYear(currentDate.Year))
            {
                return true;
            }
        }

        //Fallback
        return false;
    }
}
