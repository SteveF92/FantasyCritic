using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
internal class GameChannelReleasedGameIsRelevantTests : GameNewsChannelTests
{
    [Test]
    public void Eligible_ReleasedToday_Setting_All_NoSkippedTags() =>
        Assert.That(Setting_All_NoSkippedTags.JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_WillReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_WillReleaseInYear_NoSkippedTags.JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_MightReleaseInYear_NoSkippedTags() =>
        Assert.That(Setting_MightReleaseInYear_NoSkippedTags.JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.True);

    [Test]
    public void Eligible_ReleasedToday_Setting_Off_NoSkippedTags() =>
        Assert.That(Setting_Off_NoSkippedTags.JustReleasedGameIsRelevant(Eligible_ReleasedToday, CurrentDateForTesting), Is.False);
}
