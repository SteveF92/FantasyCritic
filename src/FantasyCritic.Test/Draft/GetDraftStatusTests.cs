using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Enums;
using NUnit.Framework;

namespace FantasyCritic.Test.Draft;

[TestFixture]
public class GetDraftStatusTests
{
    private static void AssertDraftStatus(
        DraftStatus? actual,
        LeagueDraft expectedDraft,
        DraftPhase expectedPhase,
        int expectedNextDraftPosition,
        int? expectedPreviousDraftPosition,
        int expectedDraftPosition,
        int expectedOverallDraftPosition)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Draft.DraftID, Is.EqualTo(expectedDraft.DraftID));
        Assert.That(actual.DraftPhase, Is.EqualTo(expectedPhase));
        Assert.That(
            actual.NextDraftPublisher.GetDraftPosition(expectedDraft.DraftID),
            Is.EqualTo(expectedNextDraftPosition));

        if (expectedPreviousDraftPosition is null)
        {
            Assert.That(actual.PreviousDraftPublisher, Is.Null);
        }
        else
        {
            Assert.That(actual.PreviousDraftPublisher, Is.Not.Null);
            Assert.That(
                actual.PreviousDraftPublisher!.GetDraftPosition(expectedDraft.DraftID),
                Is.EqualTo(expectedPreviousDraftPosition));
        }

        Assert.That(actual.DraftPosition, Is.EqualTo(expectedDraftPosition));
        Assert.That(actual.OverallDraftPosition, Is.EqualTo(expectedOverallDraftPosition));
    }

    [Test]
    public void GetDraftStatus_NoActiveDraft_ReturnsNull()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.NotStartedDraft)
            .Build();

        Assert.That(DraftFunctions.GetDraftStatus(leagueYear), Is.Null);
    }

    [Test]
    public void GetDraftStatus_DraftComplete_ReturnsNull()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickCounterPick()
            .PickCounterPick()
            .Build();

        Assert.That(DraftFunctions.GetDraftStatus(leagueYear), Is.Null);
    }

    [Test]
    public void GetDraftStatus_StandardGamesAtStart_ReturnsFirstPublisherUp()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.StandardGames,
            expectedNextDraftPosition: 1,
            expectedPreviousDraftPosition: null,
            expectedDraftPosition: 1,
            expectedOverallDraftPosition: 1);
    }

    [Test]
    public void GetDraftStatus_StandardGamesAfterFirstPick_ReturnsSecondPublisherUp()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.StandardGames,
            expectedNextDraftPosition: 2,
            expectedPreviousDraftPosition: 1,
            expectedDraftPosition: 1,
            expectedOverallDraftPosition: 2);
    }

    [Test]
    public void GetDraftStatus_StandardGamesEndOfFirstRound_SamePublisherIsLastAndNext()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .PickStandard()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.StandardGames,
            expectedNextDraftPosition: 2,
            expectedPreviousDraftPosition: 2,
            expectedDraftPosition: 2,
            expectedOverallDraftPosition: 3);
    }

    [Test]
    public void GetDraftStatus_StandardGamesBeforeCounterPicks_ReturnsFirstCounterPickPublisher()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.CounterPicks,
            expectedNextDraftPosition: 2,
            expectedPreviousDraftPosition: 1,
            expectedDraftPosition: 1,
            expectedOverallDraftPosition: 1);
    }

    [Test]
    public void GetDraftStatus_CounterPicksAfterFirstPick_PreviousPublisherIsLastCounterPicker()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickCounterPick()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.CounterPicks,
            expectedNextDraftPosition: 1,
            expectedPreviousDraftPosition: 2,
            expectedDraftPosition: 1,
            expectedOverallDraftPosition: 2);
    }

    [Test]
    public void GetDraftStatus_FourPlayerStandardSnake_MidSecondRound()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(4)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.StandardGames,
            expectedNextDraftPosition: 3,
            expectedPreviousDraftPosition: 4,
            expectedDraftPosition: 2,
            expectedOverallDraftPosition: 6);
    }

    [Test]
    public void GetDraftStatus_FourPlayerCounterPicksEndOfFirstRound_SamePublisherIsLastAndNext()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(4)
            .WithDraft(gamesToDraft: 1, counterPicksToDraft: 2, PlayStatus.Drafting)
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickCounterPick()
            .PickCounterPick()
            .PickCounterPick()
            .PickCounterPick()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.CounterPicks,
            expectedNextDraftPosition: 1,
            expectedPreviousDraftPosition: 1,
            expectedDraftPosition: 2,
            expectedOverallDraftPosition: 5);
    }

    [Test]
    public void GetDraftStatus_MultiDraft_UsesActiveDraftOnly()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.DraftFinal)
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickStandard()
            .PickCounterPick()
            .PickCounterPick()
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.Drafting)
            .PickStandard()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        Assert.That(activeDraft.DraftNumber, Is.EqualTo(2));

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.StandardGames,
            expectedNextDraftPosition: 2,
            expectedPreviousDraftPosition: 1,
            expectedDraftPosition: 1,
            expectedOverallDraftPosition: 2);
    }

    [Test]
    public void GetDraftStatus_DraftPaused_StillReturnsStatus()
    {
        var leagueYear = new GetDraftStatusTestBuilder()
            .WithPublishers(2)
            .WithDraft(gamesToDraft: 2, counterPicksToDraft: 1, PlayStatus.DraftPaused)
            .PickStandard()
            .Build();

        var activeDraft = leagueYear.ActiveDraft!;

        AssertDraftStatus(
            DraftFunctions.GetDraftStatus(leagueYear),
            activeDraft,
            DraftPhase.StandardGames,
            expectedNextDraftPosition: 2,
            expectedPreviousDraftPosition: 1,
            expectedDraftPosition: 1,
            expectedOverallDraftPosition: 2);
    }
}
