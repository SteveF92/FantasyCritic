using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.IntegrationTests.Tests.League.MultiDraft;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Verifies that when a publisher is auto-skipped (no open standard slots), the active draft view model
/// exposes the skip in <see cref="LeagueDraftViewModel.SkippedPicksSinceLastRealPick"/> with
/// <c>IsManualSkip=false</c>.
/// </summary>
[TestFixture]
public class DraftAutoSkipViewModelTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private LeagueYearViewModel _afterFirstPickSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.ThreePlayerAutoSkip, NewUser);

        var middlePublisher = _league.Publishers[1];
        for (var slot = 0; slot < LeagueScenarios.ThreePlayerAutoSkip.StandardGames; slot++)
            await MultiDraftTestScenario.ManagerFillOneStandardSlotAsync(_league, middlePublisher.PublisherID);

        await _league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
        });

        await _league.DraftStandardPicksAsync(1);
        _afterFirstPickSnapshot = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public void AutoSkip_NextPickIsThirdPublisher()
    {
        var activeDraft = _afterFirstPickSnapshot.ActiveDraft();
        var thirdPublisher = _afterFirstPickSnapshot.Publishers
            .Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        var expectedDisplayName = $"{thirdPublisher.PublisherName} ({thirdPublisher.PlayerName})";

        Assert.That(activeDraft?.NextPickPublisherName, Is.EqualTo(expectedDisplayName));
        Assert.That(thirdPublisher.NextToDraft, Is.True);
    }

    [Test]
    public void AutoSkip_SkippedPicksSinceLastRealPickReflectsMiddlePublisher()
    {
        var activeDraft = _afterFirstPickSnapshot.ActiveDraft();
        var middlePublisher = _afterFirstPickSnapshot.Publishers
            .Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        var expectedDisplayName = $"{middlePublisher.PublisherName} ({middlePublisher.PlayerName})";

        Assert.That(activeDraft?.SkippedPicksSinceLastRealPick, Has.Count.EqualTo(1));
        var skippedPick = activeDraft!.SkippedPicksSinceLastRealPick.Single();
        Assert.That(skippedPick.PublisherName, Is.EqualTo(expectedDisplayName));
        Assert.That(skippedPick.IsManualSkip, Is.False);
        Assert.That(skippedPick.CounterPick, Is.False);
        Assert.That(skippedPick.RoundNumber, Is.EqualTo(1));
    }
}
