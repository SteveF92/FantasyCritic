
using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class LeagueGameChannelExistingGameIsRelevantTests : LeagueGameNewsChannelTests
{

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, false, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
    Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ReleasedToday_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ReleasedToday, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_Confirmed2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_Confirmed2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_MightBe2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_MightBe2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Eligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Eligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Ineligible_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Ineligible_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Yes() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(true), ChannelKey, CurrentDateForTesting), Is.True);


    [Test]
    public void Unannounced_ConfirmedNot2025_ReleaseStatusChanged_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_No() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ExistingGameIsRelevant(Unannounced_ConfirmedNot2025, true, GetTestLeagueYear(false), ChannelKey, CurrentDateForTesting), Is.False);
}
