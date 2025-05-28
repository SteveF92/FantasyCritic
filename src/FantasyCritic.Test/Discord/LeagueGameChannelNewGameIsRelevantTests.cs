using NUnit.Framework;

namespace FantasyCritic.Test.Discord;
[TestFixture]
internal class LeagueGameChannelNewGameIsRelevantTests : LeagueGameNewsChannelTests
{
    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_Confirmed2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_MightBe2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Eligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Ineligible_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.NewGameIsRelevant(Unannounced_ConfirmedNot2025, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);
}
