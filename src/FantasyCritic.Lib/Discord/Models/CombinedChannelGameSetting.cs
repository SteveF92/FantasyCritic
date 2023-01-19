using Serilog;

namespace FantasyCritic.Lib.Discord.Models;
public class CombinedChannelGameSetting
{
    private static readonly ILogger Logger = Log.ForContext<CombinedChannelGameSetting>();

    private readonly bool _sendLeagueMasterGameUpdates;
    private readonly GameNewsSetting? _gameNewsSetting;

    public CombinedChannelGameSetting(bool sendLeagueMasterGameUpdates, GameNewsSetting? gameNewsSetting)
    {
        _sendLeagueMasterGameUpdates = sendLeagueMasterGameUpdates;
        _gameNewsSetting = gameNewsSetting;
    }

    public bool NewGameIsRelevant(MasterGame masterGame, IReadOnlyList<int> activeYears, IReadOnlyList<LeagueYear>? activeLeagueYears, DiscordChannelKey channelKey, LocalDate currentDate)
    {
        if (_gameNewsSetting is null)
        {
            //This is by definition not a game in your league (it was just added), so if you don't want general game news, then you don't want this.
            return false;
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.All))
        {
            return true;
        }

        if (activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool eligible = leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate);
                if (!eligible)
                {
                    continue;
                }

                if (_gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
                {
                    return masterGame.WillReleaseInYear(leagueYear.Year);
                }
                if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
                {
                    return masterGame.WillOrMightReleaseInYear(leagueYear.Year);
                }
            }

            return false;
        }

        if (_gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
        {
            return masterGame.WillReleaseInYears(activeYears);
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            return masterGame.WillOrMightReleaseInYears(activeYears);
        }

        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, channelKey);
        return false;
    }

    public bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, IReadOnlyList<int> activeYears,
        IReadOnlyList<LeagueYear>? activeLeagueYears, DiscordChannelKey channelKey, LocalDate currentDate)
    {
        if (activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool inLeagueYear = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
                if (inLeagueYear)
                {
                    return true;
                }

                if (_gameNewsSetting is null)
                {
                    continue;
                }

                bool eligible = leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate);
                if (!eligible)
                {
                    continue;
                }

                bool willReleaseRelevance = _gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear) && masterGame.WillReleaseInYear(leagueYear.Year);
                bool mightReleaseRelevance = _gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear) && masterGame.WillOrMightReleaseInYear(leagueYear.Year);
                bool releaseRelevance = releaseStatusChanged || willReleaseRelevance || mightReleaseRelevance;
                if (releaseRelevance)
                {
                    return true;
                }
            }

            return false;
        }

        if (_gameNewsSetting is null)
        {
            return false;
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.All))
        {
            return true;
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
        {
            return masterGame.WillReleaseInYears(activeYears);
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            return masterGame.WillOrMightReleaseInYears(activeYears);
        }

        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, channelKey);
        return false;
    }

    public bool ReleasedGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears)
    {
        if (activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool inLeagueYear = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
                if (inLeagueYear)
                {
                    return true;
                }
            }
        }

        return _gameNewsSetting is not null;
    }

    public bool ScoredGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears, bool sendNotableMisses, decimal? criticScore)
    {
        if (activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool inLeagueYear = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
                if (inLeagueYear)
                {
                    return true;
                }
            }

            if (_gameNewsSetting is not null)
            {
                return true;
            }

            if (sendNotableMisses && criticScore is >= 83m)
            {
                return true;
            }

            return false;
        }

        return _gameNewsSetting is not null;
    }

    public override string ToString()
    {
        List<string> parts = new List<string>();
        if (_sendLeagueMasterGameUpdates)
        {
            parts.Add("League Master Game Updates");
        }
        else
        {
            parts.Add("No League Master Game Updates");
        }

        if (_gameNewsSetting is null)
        {
            parts.Add("No Non-League Master Game Updates");
        }
        else if (_gameNewsSetting.Equals(GameNewsSetting.All))
        {
            parts.Add("All Master Game Updates");
        }
        else if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            parts.Add("Any 'Might Release' Master Game Updates");
        }
        else if (_gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
        {
            parts.Add("Any 'Will Release' Master Game Updates");
        }

        return string.Join(',', parts);
    }
}
