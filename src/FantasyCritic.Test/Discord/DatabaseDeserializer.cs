using System.Collections.Generic;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Test.Discord;
internal static class DatabaseDeserializer
{
    public static BaseGameNewsRelevanceHandler GetCombinedChannelGameSetting(OriginalDatabaseStructure originalStructure, LeagueYear? leagueYear = null)
    {
        var newStructure = TranslateDatabaseStructure(originalStructure);
        var translatedSetting = GameNewsSetting.GetOffSetting();
        if (newStructure.GameChannel is not null)
        {
            translatedSetting = new GameNewsSetting(){
                ShowJustReleasedAnnouncements = newStructure.GameChannel.ShowJustReleasedAnnouncements,
                ShowNewGameAnnouncements = newStructure.GameChannel.ShowNewGameAnnouncements,
                ShowAlreadyReleasedNews = newStructure.GameChannel.ShowAlreadyReleasedNews,
                ShowWillReleaseInYearNews = newStructure.GameChannel.ShowWillReleaseInYearNews,
                ShowMightReleaseInYearNews = newStructure.GameChannel.ShowMightReleaseInYearNews,
                ShowWillNotReleaseInYearNews = newStructure.GameChannel.ShowWillNotReleaseInYearNews,
                ShowScoreGameNews = newStructure.GameChannel.ShowScoreGameNews,
                ShowEditedGameNews = newStructure.GameChannel.ShowEditedGameNews
            };
        }

        if (newStructure.LeagueChannel is not null)
        {
            var leagueYearList = new List<LeagueYear>();
            if (leagueYear is not null)
            {
                leagueYearList.Add(leagueYear);
            }

            var notableMissSetting = NotableMissSetting.FromValue(newStructure.LeagueChannel.NotableMissSetting);
            return new LeagueGameNewsRelevanceHandler(newStructure.LeagueChannel.ShowPickedGameNews, newStructure.LeagueChannel.ShowEligibleGameNews,
            newStructure.LeagueChannel.ShowIneligibleGameNews, notableMissSetting, translatedSetting, newStructure.SkippedTags,
                new DiscordChannelKey(0, 0), leagueYearList);
        }

        return new GameNewsOnlyRelevanceHandler(translatedSetting, newStructure.SkippedTags, new DiscordChannelKey(0, 0));
    }

    private static NewDatabaseStructure TranslateDatabaseStructure(OriginalDatabaseStructure originalStructure)
    {
        NewGameChannel? translatedGameChannel = null;
        if (originalStructure.GameChannel is not null)
        {
            bool showAlreadyReleasedNews = originalStructure.GameChannel.GameNewsSetting == "All";
            if (originalStructure.LeagueChannel is null || originalStructure.LeagueChannel?.SendLeagueMasterGameUpdates == false)
            {
                showAlreadyReleasedNews = true;
            }

            translatedGameChannel = new NewGameChannel()
            {
                ShowJustReleasedAnnouncements = true,
                ShowNewGameAnnouncements = true,
                ShowAlreadyReleasedNews = showAlreadyReleasedNews,
                ShowWillReleaseInYearNews = true,
                ShowMightReleaseInYearNews = originalStructure.GameChannel.GameNewsSetting == "All" ||
                                             originalStructure.GameChannel.GameNewsSetting == "MightReleaseInYear",
                ShowWillNotReleaseInYearNews = originalStructure.GameChannel.GameNewsSetting == "All",
                ShowScoreGameNews = true,
                ShowEditedGameNews = true
            };
        }
        else
        {
            translatedGameChannel = new NewGameChannel()
            {
                ShowAlreadyReleasedNews = false,
                ShowWillReleaseInYearNews = false,
                ShowMightReleaseInYearNews = false,
                ShowWillNotReleaseInYearNews = false,
                ShowJustReleasedAnnouncements = false,
                ShowNewGameAnnouncements = false,
                ShowScoreGameNews = false,
                ShowEditedGameNews = false
            };
        }

        NewLeagueChannel? translatedLeagueChannel = null;
        if (originalStructure.LeagueChannel is not null)
        {
            var notableMissSetting = originalStructure.LeagueChannel.SendNotableMisses
                ? "ScoreUpdates"
                : "None";
            var showIneligibleGameNews = originalStructure.GameChannel?.GameNewsSetting == "All";
            var showEligibleGameNews = originalStructure.GameChannel is not null;

            translatedLeagueChannel = new NewLeagueChannel()
            {
                ShowPickedGameNews = originalStructure.LeagueChannel.SendLeagueMasterGameUpdates,
                ShowEligibleGameNews = showEligibleGameNews,
                ShowIneligibleGameNews = showIneligibleGameNews,
                NotableMissSetting = notableMissSetting
            };
        };

        return new NewDatabaseStructure(translatedGameChannel, translatedLeagueChannel, originalStructure.SkippedTags);
    }
}

public record OriginalDatabaseStructure(OriginalGameChannel? GameChannel, OriginalLeagueChannel? LeagueChannel, List<MasterGameTag> SkippedTags);
public record OriginalGameChannel(string GameNewsSetting);
public record OriginalLeagueChannel(bool SendLeagueMasterGameUpdates, bool SendNotableMisses);

public record NewDatabaseStructure(NewGameChannel? GameChannel, NewLeagueChannel? LeagueChannel, List<MasterGameTag> SkippedTags);
public record NewGameChannel
{
    public required bool ShowAlreadyReleasedNews {get; init;}
    public required bool ShowWillReleaseInYearNews {get; init;}
    public required bool ShowMightReleaseInYearNews {get; init;}
    public required bool ShowWillNotReleaseInYearNews { get; init; }
    public required bool ShowJustReleasedAnnouncements { get; init; }
    public required bool ShowNewGameAnnouncements { get; init; }
    public required bool ShowScoreGameNews { get; init; }
    public required bool ShowEditedGameNews { get; init; }
}

public record NewLeagueChannel
{
    public required bool ShowPickedGameNews { get; init; }
    public required bool ShowEligibleGameNews { get; init; }
    public required bool ShowIneligibleGameNews { get; init; }
    public required string NotableMissSetting { get; init; }
}
