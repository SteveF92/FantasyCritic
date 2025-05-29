using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Test.TestUtilities;
using System.Collections.Generic;

namespace FantasyCritic.Test.Discord;
internal class LeagueGameNewsChannelTests : BaseGameNewsTests
{
    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingAll_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingOff_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(true, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingAll_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_On_SettingOff_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(true, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(true, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingAll_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_On_Misses_Off_SettingOff_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(true, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(false, true), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingAll_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_On_SettingOff_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(false, true), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingAll_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting LeagueGames_Off_Misses_Off_SettingOff_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));
}
