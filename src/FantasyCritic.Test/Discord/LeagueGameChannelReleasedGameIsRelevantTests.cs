using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class LeagueGameChannelReleasedGameIsRelevantTests : LeagueGameNewsChannelTests
{
    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);

}
