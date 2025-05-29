using System.Collections.Generic;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Test.Discord;
internal static class DatabaseDeserializer
{
    public static BaseGameNewsRelevanceHandler GetCombinedChannelGameSetting(OriginalDatabaseStructure structure, LeagueYear? leagueYear = null)
    {
        var translatedSetting = GameNewsSetting.GetOffSetting();
        var hasGameChannel = structure.GameChannel is not null;
        if (hasGameChannel)
        {
            translatedSetting = new GameNewsSetting(){
                ShowJustReleasedAnnouncements = true,
                ShowNewGameAnnouncements = true,
                ShowAlreadyReleasedNews = true,
                ShowWillReleaseInYearNews = true,
                ShowMightReleaseInYearNews = structure.GameChannel!.GameNewsSetting == "All" ||
                                             structure.GameChannel.GameNewsSetting == "Might",
                ShowWillNotReleaseInYearNews = structure.GameChannel.GameNewsSetting == "All",
                ShowScoreGameNews = true,
                ShowEditedGameNews = true
            };
        }

        if (structure.LeagueChannel is not null)
        {
            var leagueYearList = new List<LeagueYear>();
            if (leagueYear is not null)
            {
                leagueYearList.Add(leagueYear);
            }
            var notableMissSetting = structure.LeagueChannel.SendNotableMisses
                ? NotableMissSetting.ScoreUpdates
                : NotableMissSetting.None;
            var showIneligibleGameNews = structure.GameChannel?.GameNewsSetting == "All";
            return new LeagueGameNewsRelevanceHandler(structure.LeagueChannel.SendLeagueMasterGameUpdates, hasGameChannel, showIneligibleGameNews, notableMissSetting, translatedSetting, structure.SkippedTags,
                new DiscordChannelKey(0, 0), leagueYearList);
        }

        return new GameNewsOnlyRelevanceHandler(translatedSetting, structure.SkippedTags, new DiscordChannelKey(0, 0));
    }
}

public record OriginalDatabaseStructure(OriginalGameChannel? GameChannel, OriginalLeagueChannel? LeagueChannel, List<MasterGameTag> SkippedTags);
public record OriginalGameChannel(string GameNewsSetting);
public record OriginalLeagueChannel(bool SendLeagueMasterGameUpdates, bool SendNotableMisses);
