using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class LeagueGameChannelScoredGameIsRelevantTests : LeagueGameNewsChannelTests
{
    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Eligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Eligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score75() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 75m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_HasGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(true), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingAll_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingAll_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingWillReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingMightReleaseInYear_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.True);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_NoSkippedTags.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_On_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_On_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_On_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_On_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

    [Test]
    public void ScoredGame_Ineligible_ReleasedToday_LeagueGames_Off_Misses_Off_SettingOff_SkipUNA_DoesNotHaveGame_Score90() =>
        Assert.That(LeagueGames_Off_Misses_Off_SettingOff_SkipUNA.ScoredGameIsRelevant(Ineligible_ReleasedToday, GetTestLeagueYear(false), 90m, CurrentDateForTesting), Is.False);

}
