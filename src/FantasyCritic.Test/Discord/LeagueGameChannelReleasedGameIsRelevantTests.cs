using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class LeagueGameChannelReleasedGameIsRelevantTests : LeagueGameNewsChannelTests
{
    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.True);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true)), Is.False);

    [Test]
    public void Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ReleasedGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false)), Is.False);

}
