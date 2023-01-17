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
        throw new NotImplementedException();
    }

    public bool ExistingGameIsRelevant(MasterGameYear masterGameYear, bool releaseStatusChanged, IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID)
    {
        throw new NotImplementedException();
    }

    public bool ScoredOrReleasedGameIsRelevant(IReadOnlySet<Guid> leaguesWithGame, Guid? leagueID)
    {
        throw new NotImplementedException();
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
