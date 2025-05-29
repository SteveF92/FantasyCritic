using NUnit.Framework;

namespace FantasyCritic.Test.Discord;
[TestFixture]
internal class GameChannelNewGameIsRelevantTests : GameNewsChannelTests
{

    // ========== Eligible_Confirmed2025 ==========
    [Test]
    public void EligibleConfirmed2025_SettingAll_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmed2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmed2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmed2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleConfirmed2025_SettingAll_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmed2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmed2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmed2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, CurrentDateForTesting), Is.False);


    // ========== Eligible_MightBe2025 ==========
    [Test]
    public void EligibleMightBe2025_SettingAll_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleMightBe2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleMightBe2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleMightBe2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleMightBe2025_SettingAll_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleMightBe2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleMightBe2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleMightBe2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, CurrentDateForTesting), Is.False);


    // ========== Eligible_ConfirmedNot2025 ==========
    [Test]
    public void EligibleConfirmedNot2025_SettingAll_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmedNot2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleConfirmedNot2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleConfirmedNot2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleConfirmedNot2025_SettingAll_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.True);

    [Test]
    public void EligibleConfirmedNot2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleConfirmedNot2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void EligibleConfirmedNot2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, CurrentDateForTesting), Is.False);


    // ========== Unannounced_MightBe2025 ==========
    [Test]
    public void UnannouncedMightBe2025_SettingAll_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void UnannouncedMightBe2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedMightBe2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void UnannouncedMightBe2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedMightBe2025_SettingAll_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.True);

    [Test]
    public void UnannouncedMightBe2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedMightBe2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedMightBe2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, CurrentDateForTesting), Is.False);


    // ========== Unannounced_ConfirmedNot2025 ==========
    [Test]
    public void UnannouncedConfirmedNot2025_SettingAll_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.True);

    [Test]
    public void UnannouncedConfirmedNot2025_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedConfirmedNot2025_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedConfirmedNot2025_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedConfirmedNot2025_SettingAll_SkipUNA() =>
        Assert.That(Setting_All_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.True);

    [Test]
    public void UnannouncedConfirmedNot2025_Setting_WillReleaseInYear_SkipUNA() =>
        Assert.That(Setting_WillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedConfirmedNot2025_Setting_MightReleaseInYear_SkipUNA() =>
        Assert.That(Setting_MightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.False);

    [Test]
    public void UnannouncedConfirmedNot2025_Setting_Off_SkipUNA() =>
        Assert.That(Setting_Off_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, CurrentDateForTesting), Is.False);
}
