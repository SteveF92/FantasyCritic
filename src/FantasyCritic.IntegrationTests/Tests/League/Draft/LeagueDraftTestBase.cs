using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft;

/// <summary>
/// Abstract base for draft scenario tests. Subclasses provide a <see cref="LeagueScenario"/>
/// and optionally override <see cref="SimulateDraftAsync"/> to change how the draft is run
/// (e.g. player-side vs manager-side).
///
/// <see cref="SetUpLeagueAndDraft"/> builds the complete state; the shared <c>[Test]</c> methods
/// assert against the post-draft <see cref="LeagueYearSnapshot"/>.
/// </summary>
public abstract class LeagueDraftTestBase : IntegrationTestBase
{
    protected abstract LeagueScenario Scenario { get; }

    protected LeagueFixture League = null!;
    protected LeagueYearViewModel LeagueYearSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpLeagueAndDraft()
    {
        League = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, Scenario, NewUser);
        await SimulateDraftAsync();
        LeagueYearSnapshot = await League.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownSessions() => await League.DisposeAsync();

    /// <summary>
    /// Runs the draft to completion using <see cref="MockedLivePlayer"/> / <see cref="DraftSimulator"/>.
    /// Override to use the manager-side <c>ManagerDraftGame</c> endpoint instead.
    /// </summary>
    protected virtual Task SimulateDraftAsync() => League.DraftToCompletionAsync();

    [Test]
    public void Draft_Completed_PlayStatusShowsDraftFinished()
    {
        Assert.That(LeagueYearSnapshot.PlayStatus.DraftFinished, Is.True,
            "Draft must be finished after simulation.");
    }

    [Test]
    public void Draft_Completed_DraftIsNoLongerActive()
    {
        Assert.That(LeagueYearSnapshot.PlayStatus.DraftIsActive, Is.False);
    }

    [Test]
    public void Draft_Completed_NoPublisherIsNextToDraft()
    {
        Assert.That(
            LeagueYearSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be marked NextToDraft once the draft is finished.");
    }

    [Test]
    public void Draft_Completed_AllPublishersHaveCorrectStandardGameCount()
    {
        foreach (var publisher in LeagueYearSnapshot.Publishers)
        {
            var standardGames = publisher.Games.Count(g => !g.CounterPick);
            Assert.That(standardGames, Is.EqualTo(Scenario.GamesToDraft),
                $"Publisher '{publisher.PublisherName}' should have {Scenario.GamesToDraft} standard games.");
        }
    }

    [Test]
    public void Draft_Completed_AllPublishersHaveCorrectCounterPickCount()
    {
        foreach (var publisher in LeagueYearSnapshot.Publishers)
        {
            var counterPicks = publisher.Games.Count(g => g.CounterPick);
            Assert.That(counterPicks, Is.EqualTo(Scenario.CounterPicksToDraft),
                $"Publisher '{publisher.PublisherName}' should have {Scenario.CounterPicksToDraft} counter-pick(s).");
        }
    }

    [Test]
    public void Draft_Completed_TotalGamesAcrossAllPublishers_EqualsExpectedCount()
    {
        int expectedTotal =
            Scenario.PlayerCount * (Scenario.GamesToDraft + Scenario.CounterPicksToDraft);
        int actualTotal = LeagueYearSnapshot.Publishers.Sum(p => p.Games.Count);

        Assert.That(actualTotal, Is.EqualTo(expectedTotal),
            $"Total games across all publishers should be {expectedTotal}.");
    }
}
