using NUnit.Framework;

namespace FantasyCritic.Test.Discord;
[TestFixture]
internal class GameChannelScoredGameIsRelevantTests : GameNewsChannelTests
{
    // ========== Eligible_ReleasedToday with Score = 75 ==========
    [Test] public void Eligible_ReleasedToday_Setting_All_NoSkippedTags_Score75() =>
        Assert.That(Setting_All_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test] public void Eligible_ReleasedToday_Setting_WillReleaseInYear_NoSkippedTags_Score75() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test] public void Eligible_ReleasedToday_Setting_MightReleaseInYear_NoSkippedTags_Score75() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.True);

    [Test] public void Eligible_ReleasedToday_Setting_Off_NoSkippedTags_Score75() =>
        Assert.That(Setting_Off_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 75m, CurrentDateForTesting), Is.False);

    // ========== Eligible_ReleasedToday with Score = 90 ==========
    [Test] public void Eligible_ReleasedToday_Setting_All_NoSkippedTags_Score90() =>
        Assert.That(Setting_All_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test] public void Eligible_ReleasedToday_Setting_WillReleaseInYear_NoSkippedTags_Score90() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test] public void Eligible_ReleasedToday_Setting_MightReleaseInYear_NoSkippedTags_Score90() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.True);

    [Test] public void Eligible_ReleasedToday_Setting_Off_NoSkippedTags_Score90() =>
        Assert.That(Setting_Off_NoSkippedTags.ScoredGameIsRelevant(Eligible_ReleasedToday, null, 90m, CurrentDateForTesting), Is.False);

}