using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class LeagueGameChannelScoredGameIsRelevantTests : LeagueGameNewsChannelTests
{
    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    //This should not exist, there should not be a way for the user to have turned on notable miss settings, turn off game news, turn off league games
    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    //This should not exist, there should not be a way for the user to have turned on notable miss settings, turn off game news, turn off league games
    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    //This should not exist, there should not be a way for the user to have turned on notable miss settings, turn off game news, turn off league games
    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    //This should not exist, there should not be a way for the user to have turned on notable miss settings, turn off game news, turn off league games
    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(true)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA(GetTestLeagueYear(false)).ScoredGameIsRelevant(Ineligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

}
