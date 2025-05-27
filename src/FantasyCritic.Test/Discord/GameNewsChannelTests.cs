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
        new CombinedChannelGameSetting(false, false, GameNewsSetting.All, new List<MasterGameTag>());

    public static readonly CombinedChannelGameSetting Setting_WillReleaseInYear_NoSkippedTags =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.WillReleaseInYear, new List<MasterGameTag>());

    public static readonly CombinedChannelGameSetting Setting_MightReleaseInYear_NoSkippedTags =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.MightReleaseInYear, new List<MasterGameTag>());

    public static readonly CombinedChannelGameSetting Setting_Off_NoSkippedTags =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.Off, new List<MasterGameTag>());

    // Skipped tag: UNA (Unannounced game)
    public static readonly CombinedChannelGameSetting Setting_All_SkipUNA =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.All, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] });

    public static readonly CombinedChannelGameSetting Setting_WillReleaseInYear_SkipUNA =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.WillReleaseInYear, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] });

    public static readonly CombinedChannelGameSetting Setting_MightReleaseInYear_SkipUNA =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.MightReleaseInYear, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] });

    public static readonly CombinedChannelGameSetting Setting_Off_SkipUNA =
        new CombinedChannelGameSetting(false, false, GameNewsSetting.Off, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] });

    [Test]
    public void ConfirmedGame_IsRelevant_WhenSettingIsAll_AndNoTagsSkipped()
    {
        var setting = Setting_All_NoSkippedTags;
        var result = setting.NewGameIsRelevant(Eligible_Future_Confirmed2025, null, ChannelKey, CurrentDateForTesting);
        Assert.That(result, Is.True);
    }
}
