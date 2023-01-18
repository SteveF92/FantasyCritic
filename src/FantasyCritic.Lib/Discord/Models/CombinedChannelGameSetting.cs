namespace FantasyCritic.Lib.Discord.Models;
public class CombinedChannelGameSetting
{
    private readonly bool _sendLeagueMasterGameUpdates;
    private readonly GameNewsSetting? _gameNewsSetting;

    public CombinedChannelGameSetting(bool sendLeagueMasterGameUpdates, GameNewsSetting? gameNewsSetting)
    {
        _sendLeagueMasterGameUpdates = sendLeagueMasterGameUpdates;
        _gameNewsSetting = gameNewsSetting;
    }

    public bool NewGameIsRelevant(MasterGame masterGame, int year)
    {
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
            return masterGame.WillReleaseInYear(year).Equals(WillReleaseStatus.WillRelease);
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            return masterGame.WillReleaseInYear(year).Equals(WillReleaseStatus.WillRelease) || masterGame.WillReleaseInYear(year).Equals(WillReleaseStatus.MightRelease);
        }
        
        throw new Exception($"Invalid game news value: {_gameNewsSetting}");
    }

    public bool ExistingGameIsRelevant(MasterGameYear masterGameYear, int year, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID)
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
            return masterGameYear.MasterGame.WillReleaseInYear(year).Equals(WillReleaseStatus.WillRelease);
        }
        if (_gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            return masterGameYear.MasterGame.WillReleaseInYear(year).Equals(WillReleaseStatus.WillRelease) || masterGameYear.MasterGame.WillReleaseInYear(year).Equals(WillReleaseStatus.MightRelease);
        }

        throw new Exception($"Invalid game news value: {_gameNewsSetting}");
    }

    public bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID)
    {
        if (_sendLeagueMasterGameUpdates && leagueID.HasValue && leaguesWithGame.Contains(leagueID.Value))
        {
            return true;
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
