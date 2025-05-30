
using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class LeagueGameChannelExistingGameIsRelevantTests : LeagueGameNewsChannelTests
{

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ReleasedToday, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_Confirmed2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_MightBe2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);

    //If setting is all, then it should not skip the game even if skipped tags are present
    //[Test]
    //public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
    //    Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, CurrentDateForTesting), Is.False);

}
