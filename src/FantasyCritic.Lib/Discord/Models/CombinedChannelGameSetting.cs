using Serilog;

namespace FantasyCritic.Lib.Discord.Models;
public class CombinedChannelGameSetting
{
    private static readonly ILogger Logger = Log.ForContext<CombinedChannelGameSetting>();

    private readonly bool _showPickedGameNews;
    private readonly bool _showEligibleGameNews;
    private readonly NotableMissSetting _notableMissSetting;
    private readonly GameNewsSetting _gameNewsSetting;
    private readonly IReadOnlyList<MasterGameTag> _skippedTags;

    public CombinedChannelGameSetting(bool showPickedGameNews, bool showEligibleGameNews, NotableMissSetting notableMissSetting, GameNewsSetting gameNewsSetting, IReadOnlyList<MasterGameTag> skippedTags)
    {
        _showPickedGameNews = showPickedGameNews;
        _showEligibleGameNews = showEligibleGameNews;
        _notableMissSetting = notableMissSetting;
        _gameNewsSetting = gameNewsSetting;
        _skippedTags = skippedTags;
    }

    public bool NewGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears, DiscordChannelKey channelKey, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, IReadOnlyList<LeagueYear>? activeLeagueYears,
        DiscordChannelKey channelKey, LocalDate currentDate)
    {
        throw new NotImplementedException();
    }

    public bool ReleasedGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears)
    {
        throw new NotImplementedException();
    }

    public bool ScoredGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears, decimal? criticScore, LocalDate currentDate)
    {
        throw new NotImplementedException();
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

        //Fallback
        return false;
    }
}
