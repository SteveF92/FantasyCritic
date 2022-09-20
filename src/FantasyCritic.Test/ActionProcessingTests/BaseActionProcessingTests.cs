using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Test.TestUtilities;
using NodaTime;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

namespace FantasyCritic.Test.ActionProcessingTests;

public abstract class BaseActionProcessingTests
{
    protected FinalizedActionProcessingResults _results = null!;

    protected void SetupAndProcess(Instant processingTime, string testDataName)
    {
        VerifierSettings.DontScrubGuids();
        var currentDate = processingTime.InZone(TimeExtensions.EasternTimeZone).Date;

        var testDataService = new TestDataService($"../../../TestData/{testDataName}/");
        var systemWideValues = testDataService.GetSystemWideValues();
        var masterGameYearDictionary = testDataService.GetMasterGameYears();
        var publishers = testDataService.GetPublishers(masterGameYearDictionary);
        var masterGameTags = testDataService.GetTags();
        var leagueYears = testDataService.GetLeagueYears(publishers, masterGameYearDictionary, masterGameTags);
        var allActiveBids = testDataService.GetActiveBids(leagueYears, masterGameYearDictionary);
        var allActiveDrops = testDataService.GetActiveDrops(leagueYears, masterGameYearDictionary);

        _results = ActionProcessingFunctions.ProcessActions(systemWideValues, allActiveBids, allActiveDrops, publishers, processingTime, currentDate, masterGameYearDictionary);
    }

    [Test]
    public Task SuccessBidsTest()
    {
        var testData = _results.Results.SuccessBids.Select(x => new
            {
                x.PickupBid.BidID,
                x.PickupBid.Publisher.PublisherID,
                x.PickupBid.MasterGame.MasterGameID,
                x.PickupBid.CounterPick,
                x.PickupBid.Priority,
                x.PickupBid.BidAmount,
                x.ProjectedPointsAtTimeOfBid,
                x.SlotNumber,
                x.Outcome,
                ConditionalDropMasterGameID = x.PickupBid.ConditionalDropPublisherGame?.MasterGame!.MasterGame.MasterGameID,
                ConditionalDropResult = x.PickupBid.ConditionalDropResult?.Result.ToString()
            })
            .OrderBy(x => x.BidID)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task FailedBidsTest()
    {
        var testData = _results.Results.FailedBids.Select(x => new
            {
                x.PickupBid.BidID,
                x.PickupBid.Publisher.PublisherID,
                x.PickupBid.MasterGame.MasterGameID,
                x.PickupBid.BidAmount,
                x.ProjectedPointsAtTimeOfBid,
                x.FailureReason,
                x.Outcome
            })
            .OrderBy(x => x.BidID)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task SuccessDropsTest()
    {
        var testData = _results.Results.SuccessDrops.Select(x => new
            {
                x.DropRequestID,
                x.Publisher.PublisherID,
                x.MasterGame.MasterGameID,
            })
            .OrderBy(x => x.DropRequestID)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task FailedDropsTest()
    {
        var testData = _results.Results.FailedDrops.Select(x => new
            {
                x.DropRequestID,
                x.Publisher.PublisherID,
                x.MasterGame.MasterGameID,
            })
            .OrderBy(x => x.DropRequestID)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task AddedPublisherGamesTest()
    {
        var testData = _results.Results.AddedPublisherGames.Select(x => new
            {
                x.PublisherID,
                x.MasterGame!.MasterGame.MasterGameID,
                x.CounterPick,
                x.SlotNumber,
                x.BidAmount
            })
            .OrderBy(x => x.PublisherID)
            .ThenBy(x => x.MasterGameID)
            .ThenBy(x => x.CounterPick)
            .ThenBy(x => x.SlotNumber)
            .ThenBy(x => x.BidAmount)
            .ToList();
        return Verifier.Verify(testData);
    }

    [Test]
    public Task RemovedPublisherGamesTest()
    {
        var testData = _results.Results.RemovedPublisherGames.Select(x => new
            {
                x.PublisherGame.PublisherID,
                x.PublisherGame.MasterGame!.MasterGame.MasterGameID,
                x.PublisherGame.CounterPick,
                x.PublisherGame.SlotNumber,
                x.PublisherGame.BidAmount,
                x.RemovedNote
            })
            .OrderBy(x => x.PublisherID)
            .ThenBy(x => x.MasterGameID)
            .ThenBy(x => x.CounterPick)
            .ThenBy(x => x.SlotNumber)
            .ThenBy(x => x.BidAmount)
            .ThenBy(x => x.RemovedNote)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task LeagueActionsTest()
    {
        var testData = _results.Results.LeagueActions.Select(x => new
            {
                x.Publisher.LeagueYearKey.LeagueID,
                x.Publisher.PublisherID,
                x.ActionType,
                x.Description
            })
            .OrderBy(x => x.LeagueID)
            .ThenBy(x => x.PublisherID)
            .ThenBy(x => x.ActionType)
            .ThenBy(x => x.Description)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public void NoSpecialAuctionsTest()
    {
        Assert.AreEqual(0, _results.SpecialAuctionsProcessed.Count);
    }
}
