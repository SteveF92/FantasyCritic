using System.Collections.Generic;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Test.TestUtilities;
using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class GameNewsChannelTests : BaseGameNewsTests
{
    // No skipped tags
    public static readonly CombinedChannelGameSetting Setting_All_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting Setting_WillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting Setting_MightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    public static readonly CombinedChannelGameSetting Setting_Off_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(false, false), new List<MasterGameTag>()));

    // Skipped tag: UNA (Unannounced game)
    public static readonly CombinedChannelGameSetting Setting_All_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting Setting_WillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting Setting_MightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly CombinedChannelGameSetting Setting_Off_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("Off"), new OriginalLeagueChannel(false, false), new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));


    [Test]
    public void NewGameIsRelevant_EligibleConfirmed2025_SettingAll_NoSkippedTags()
    {
        var setting = Setting_All_NoSkippedTags;
        var result = setting.NewGameIsRelevant(Eligible_Confirmed2025, null, ChannelKey, CurrentDateForTesting);
        Assert.That(result, Is.True);
    }

    [Test]
    public void ExistingGameIsRelevant_EligibleConfirmed2025_SettingAll_NoSkippedTags()
    {
        var setting = Setting_All_NoSkippedTags;
        var result = setting.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting);
        Assert.That(result, Is.True);
    }

    [Test]
    public void ReleasedGameIsRelevant_EligibleConfirmed2025_SettingAll_NoSkippedTags()
    {
        var setting = Setting_All_NoSkippedTags;
        var result = setting.ReleasedGameIsRelevant(Eligible_ReleasedToday_NoScore, null);
        Assert.That(result, Is.True);
    }

    [Test]
    public void ReleasedGameIsRelevant_Eligible_ReleasedToday_Score75_SettingAll_NoSkippedTags()
    {
        var setting = Setting_All_NoSkippedTags;
        var result = setting.ScoredGameIsRelevant(Eligible_ReleasedToday_Score75, null, 75m, CurrentDateForTesting);
        Assert.That(result, Is.True);
    }
}
