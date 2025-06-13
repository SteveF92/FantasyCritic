using FantasyCritic.Lib.Enums;
using NUnit.Framework;

namespace FantasyCritic.Test.Discord;
[TestFixture]
internal class GameChannelExistingGameIsRelevantTests : GameNewsChannelTests
{
    // ========== Eligible_ReleasedToday ==========

    [Test]
    public void Eligible_ReleasedToday_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, null, CurrentDateForTesting), Is.False);

    // ========== Eligible_Confirmed2025 ==========

    [Test]
    public void Eligible_Confirmed2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, null, CurrentDateForTesting), Is.False);

    // ========== Eligible_MightBe2025 ==========

    [Test]
    public void Eligible_MightBe2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, null, CurrentDateForTesting), Is.False);

    // ========== Eligible_ConfirmedNot2025 ==========

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);


    // ========== Unannounced_MightBe2025 ==========

    [Test]
    public void Unannounced_MightBe2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, null, CurrentDateForTesting), Is.False);

    // ========== Unannounced_ConfirmedNot2025 ==========

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, null, CurrentDateForTesting), Is.False);

    // ========== Eligible_ReleasedToday ==========

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    // ========== Eligible_Confirmed2025 ==========

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    // ========== Eligible_MightBe2025 ==========

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    // ========== Eligible_ConfirmedNot2025 ==========

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);


    // ========== Unannounced_MightBe2025 ==========

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    // ========== Unannounced_ConfirmedNot2025 ==========

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_All_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, WillReleaseStatus.MightRelease, CurrentDateForTesting), Is.False);
}
