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
        var hasGameChannel = structure.GameChannel.GameNewsSetting != "Off";
        var hasLeagueChannel = structure.LeagueChannel is not null;
        if (hasGameChannel)
        {
            bool showReleasedGameNews = structure.GameChannel.GameNewsSetting == "All";
            if (!hasLeagueChannel || structure.LeagueChannel?.SendLeagueMasterGameUpdates == false)
            {
                showReleasedGameNews = true;
            }

            translatedSetting = new GameNewsSetting(){
                ShowJustReleasedAnnouncements = showReleasedGameNews,
                ShowNewGameAnnouncements = true,
                ShowAlreadyReleasedNews = showReleasedGameNews,
                ShowWillReleaseInYearNews = true,
                ShowMightReleaseInYearNews = structure.GameChannel!.GameNewsSetting == "All" ||
                                             structure.GameChannel.GameNewsSetting == "MightReleaseInYear",
                ShowWillNotReleaseInYearNews = structure.GameChannel.GameNewsSetting == "All",
                ShowScoreGameNews = showReleasedGameNews,
                ShowEditedGameNews = showReleasedGameNews
            };
        }

        if (hasLeagueChannel)
        {
            var leagueYearList = new List<LeagueYear>();
            if (leagueYear is not null)
            {
                leagueYearList.Add(leagueYear);
            }
            var notableMissSetting = (structure.LeagueChannel?.SendNotableMisses ?? true)
                ? NotableMissSetting.ScoreUpdates
                : NotableMissSetting.None;
            var showIneligibleGameNews = structure.GameChannel.GameNewsSetting == "All";
            return new LeagueGameNewsRelevanceHandler(structure.LeagueChannel!.SendLeagueMasterGameUpdates, hasGameChannel, showIneligibleGameNews, notableMissSetting, translatedSetting, structure.SkippedTags,
                new DiscordChannelKey(0, 0), leagueYearList);
        }

        return new GameNewsOnlyRelevanceHandler(translatedSetting, structure.SkippedTags, new DiscordChannelKey(0, 0));
    }
}

public record OriginalDatabaseStructure(OriginalGameChannel GameChannel, OriginalLeagueChannel? LeagueChannel, List<MasterGameTag> SkippedTags);
public record OriginalGameChannel(string GameNewsSetting);
public record OriginalLeagueChannel(bool SendLeagueMasterGameUpdates, bool SendNotableMisses);
