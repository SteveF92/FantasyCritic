using NUnit.Framework;

namespace FantasyCritic.Test.Discord;
[TestFixture]
internal class GameChannelExistingGameIsRelevantTests : GameNewsChannelTests
{
    // ========== Eligible_ReleasedLastWeek ==========

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedLastWeek_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Eligible_ReleasedToday ==========

    [Test]
    public void Eligible_ReleasedToday_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Eligible_Confirmed2025 ==========

    [Test]
    public void Eligible_Confirmed2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Eligible_MightBe2025 ==========

    [Test]
    public void Eligible_MightBe2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Eligible_ConfirmedNot2025 ==========

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Unannounced_ReleasedLastWeek ==========

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ReleasedLastWeek_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedLastWeek, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Unannounced_ReleasedToday ==========

    [Test]
    public void Unannounced_ReleasedToday_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedToday_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedToday_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedToday_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ReleasedToday_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ReleasedToday_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ReleasedToday_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ReleasedToday_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_ReleasedToday, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Unannounced_Confirmed2025 ==========

    [Test]
    public void Unannounced_Confirmed2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_Confirmed2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_Confirmed2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_Confirmed2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_Confirmed2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_Confirmed2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_Confirmed2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_Confirmed2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_Confirmed2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Unannounced_MightBe2025 ==========

    [Test]
    public void Unannounced_MightBe2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    // ========== Unannounced_ConfirmedNot2025 ==========

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, null, ChannelKey, CurrentDateForTesting), Is.False);
}
