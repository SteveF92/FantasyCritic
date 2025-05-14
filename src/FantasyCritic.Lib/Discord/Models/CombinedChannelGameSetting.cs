using Serilog;

namespace FantasyCritic.Lib.Discord.Models;
public class CombinedChannelGameSetting
{
    private static readonly ILogger Logger = Log.ForContext<CombinedChannelGameSetting>();

    private readonly bool _sendLeagueMasterGameUpdates;
    private readonly bool _sendNotableMisses;
    private readonly GameNewsSettingOld _gameNewsSettingOld;
    private readonly IReadOnlyList<MasterGameTag> _skippedTags;

    public CombinedChannelGameSetting(bool sendLeagueMasterGameUpdates, bool sendNotableMisses, GameNewsSettingOld gameNewsSettingOld, IReadOnlyList<MasterGameTag> skippedTags)
    {
        _sendLeagueMasterGameUpdates = sendLeagueMasterGameUpdates;
        _sendNotableMisses = sendNotableMisses;
        _gameNewsSettingOld = gameNewsSettingOld;
        _skippedTags = skippedTags;
    }

    public bool NewGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears, DiscordChannelKey channelKey, LocalDate currentDate)
    {
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.Off))
        {
            //This is by definition not a game in your league (it was just added), so if you don't want general game news, then you don't want this.
            return false;
        }
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.All))
        {
            return true;
        }

        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
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

                if (_gameNewsSettingOld.Equals(GameNewsSettingOld.WillReleaseInYear))
                {
                    return masterGame.WillReleaseInYear(leagueYear.Year);
                }
                if (_gameNewsSettingOld.Equals(GameNewsSettingOld.MightReleaseInYear))
                {
                    return masterGame.WillOrMightReleaseInYear(leagueYear.Year);
                }
            }

            return false;
        }

        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.WillReleaseInYear))
        {
            return masterGame.WillReleaseInYear(currentDate.Year);
        }
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.MightReleaseInYear))
        {
            return masterGame.WillOrMightReleaseInYear(currentDate.Year);
        }

        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, channelKey);
        return false;
    }

    public bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, IReadOnlyList<LeagueYear>? activeLeagueYears,
        DiscordChannelKey channelKey, LocalDate currentDate)
    {
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.All))
        {
            return true;
        }

        if (activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool inLeagueYear = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
                if (_sendLeagueMasterGameUpdates && inLeagueYear)
                {
                    return true;
                }

                if (_gameNewsSettingOld.Equals(GameNewsSettingOld.Off))
                {
                    continue;
                }

                if (masterGame.Tags.Intersect(_skippedTags).Any())
                {
                    continue;
                }

                bool eligible = leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate);
                if (!eligible)
                {
                    continue;
                }

                bool willReleaseRelevance = _gameNewsSettingOld.Equals(GameNewsSettingOld.WillReleaseInYear) && masterGame.WillReleaseInYear(leagueYear.Year);
                bool mightReleaseRelevance = _gameNewsSettingOld.Equals(GameNewsSettingOld.MightReleaseInYear) && masterGame.WillOrMightReleaseInYear(leagueYear.Year);
                bool releaseRelevance = releaseStatusChanged || willReleaseRelevance || mightReleaseRelevance;
                if (releaseRelevance)
                {
                    return true;
                }
            }

            return false;
        }

        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.Off))
        {
            return false;
        }
        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.WillReleaseInYear))
        {
            return masterGame.WillReleaseInYear(currentDate.Year);
        }
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.MightReleaseInYear))
        {
            return masterGame.WillOrMightReleaseInYear(currentDate.Year);
        }

        Logger.Warning("Invalid game news configuration for: {gameName}, {channelKey}", masterGame.GameName, channelKey);
        return false;
    }

    public bool ReleasedGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears)
    {
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.All))
        {
            return true;
        }

        if (_sendLeagueMasterGameUpdates && activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool inLeagueYear = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
                if (inLeagueYear)
                {
                    return true;
                }
            }

            return false;
        }

        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        return !_gameNewsSettingOld.Equals(GameNewsSettingOld.Off);
    }

    public bool ScoredGameIsRelevant(MasterGame masterGame, IReadOnlyList<LeagueYear>? activeLeagueYears, decimal? criticScore, LocalDate currentDate)
    {
        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.All))
        {
            return true;
        }

        if (_sendLeagueMasterGameUpdates && activeLeagueYears is not null)
        {
            foreach (var leagueYear in activeLeagueYears)
            {
                bool inLeagueYear = leagueYear.Publishers.Any(x => x.MyMasterGames.Contains(masterGame));
                if (inLeagueYear)
                {
                    return true;
                }

                if (_sendNotableMisses && criticScore is >= 83m && leagueYear.GameIsEligibleInAnySlot(masterGame, currentDate))
                {
                    return true;
                }
            }

            return false;
        }

        if (masterGame.Tags.Intersect(_skippedTags).Any())
        {
            return false;
        }

        return !_gameNewsSettingOld.Equals(GameNewsSettingOld.Off);
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

        if (_gameNewsSettingOld.Equals(GameNewsSettingOld.Off))
        {
            parts.Add("No Non-League Master Game Updates");
        }
        else if (_gameNewsSettingOld.Equals(GameNewsSettingOld.All))
        {
            parts.Add("All Master Game Updates");
        }
        else if (_gameNewsSettingOld.Equals(GameNewsSettingOld.MightReleaseInYear))
        {
            parts.Add("Any 'Might Release' Master Game Updates");
        }
        else if (_gameNewsSettingOld.Equals(GameNewsSettingOld.WillReleaseInYear))
        {
            parts.Add("Any 'Will Release' Master Game Updates");
        }

        return string.Join(',', parts);
    }
}
