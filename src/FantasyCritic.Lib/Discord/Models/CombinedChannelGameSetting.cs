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
        }
        else
        {
            if (_gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
            {
                return masterGame.WillReleaseInYears(activeYears);
            }
            if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
            {
                return masterGame.WillOrMightReleaseInYears(activeYears);
            }
        }

        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, channelKey);
        return false;
    }

    public bool ExistingGameIsRelevant(MasterGameYear masterGameYear, IReadOnlyList<int> activeYears, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID)
    {
        if (_sendLeagueMasterGameUpdates && leagueID.HasValue && leaguesWithGame.Contains(leagueID.Value))
        {
            return true;
        }

        if (_gameNewsSetting is null)
        {
            return false;
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.All))
        {
            return true;
        }
        if (releaseStatusChanged)
        {
            return true;
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
        {
            return masterGameYear.MasterGame.WillReleaseInYears(activeYears);
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            return masterGameYear.MasterGame.WillOrMightReleaseInYears(activeYears);
        }

        throw new Exception($"Invalid game news value: {_gameNewsSetting}");
    }

    public bool ReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID)
    {
        if (leagueID.HasValue)
        {
            if (!_sendLeagueMasterGameUpdates)
            {
                return false;
            }

            return leaguesWithGame.Contains(leagueID.Value);
        }

        if (_gameNewsSetting is null)
        {
            return false;
        }

        return true;
    }

    public bool ScoredGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID, bool sendNotableMisses, decimal? criticScore)
    {
        if (leagueID.HasValue)
        {
            if (!_sendLeagueMasterGameUpdates)
            {
                return false;
            }

            if (leaguesWithGame.Contains(leagueID.Value))
            {
                return true;
            }

            if (sendNotableMisses && criticScore is >= 83m)
            {
                return true;
            }

            return false;
        }

        if (_gameNewsSetting is null)
        {
            return false;
        }

        return true;
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
