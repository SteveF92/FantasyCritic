using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Test.TestUtilities;
using System.Collections.Generic;

namespace FantasyCritic.Test.Discord;
internal class LeagueGameNewsChannelTests : BaseGameNewsTests
{
    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(true, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingAll_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_On_SettingOff_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(true, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingAll_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_On_Misses_Off_SettingOff_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(false, true), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingAll_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_On_SettingOff_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(false, false), new List<MasterGameTag>()), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);

    public static BaseGameNewsRelevanceHandler LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(LeagueYear leagueYear) =>
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }), leagueYear);
}
