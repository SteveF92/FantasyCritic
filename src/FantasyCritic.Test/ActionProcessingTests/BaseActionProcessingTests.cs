using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Test.TestUtilities;
using NodaTime;
using NUnit.Framework;
using VerifyNUnit;

namespace FantasyCritic.Test.ActionProcessingTests;

public abstract class BaseActionProcessingTests
{
    private readonly Dictionary<string, FinalizedActionProcessingResults> _results = new Dictionary<string, FinalizedActionProcessingResults>();

    protected abstract Instant ProcessingTime { get; }
    protected abstract string ActionProcessingSetName { get; }
    protected abstract bool DefaultAllowIneligible { get; }
    protected virtual Guid? OnlyRunLeagueID => null;

    [OneTimeSetUp]
    public void Process()
    {
        var currentDate = ProcessingTime.InZone(TimeExtensions.EasternTimeZone).Date;

        var testDataService = new TestDataService($"../../../TestData/{ActionProcessingSetName}/", DefaultAllowIneligible);
        var systemWideValues = testDataService.GetSystemWideValues();
        var masterGameYearDictionary = testDataService.GetMasterGameYears();
        var publishers = testDataService.GetPublishers(masterGameYearDictionary);
        var masterGameTags = testDataService.GetTags();
        var leagueYears = testDataService.GetLeagueYears(publishers, masterGameYearDictionary, masterGameTags);
        var allActiveBids = testDataService.GetActiveBids(leagueYears, masterGameYearDictionary);
        var allActiveDrops = testDataService.GetActiveDrops(leagueYears, masterGameYearDictionary);

        if (OnlyRunLeagueID.HasValue)
        {
            allActiveBids = allActiveBids
                .Where(x => x.Key.League.LeagueID == OnlyRunLeagueID.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            allActiveDrops = allActiveDrops
                .Where(x => x.Key.League.LeagueID == OnlyRunLeagueID.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        var actionProcessor = new ActionProcessor(systemWideValues, ProcessingTime, currentDate, masterGameYearDictionary);
        _results[ActionProcessingSetName] = actionProcessor.ProcessActions(allActiveBids, allActiveDrops, publishers);

        Verifier.UseProjectRelativeDirectory("./ActionProcessingTests/Expected");
    }

    [Test]
    public Task SuccessBidsTest()
    {
        var testData = _results[ActionProcessingSetName].Results.SuccessBids.Select(x => new
            {
                x.PickupBid.BidID,
                x.PickupBid.Publisher.PublisherID,
                x.PickupBid.MasterGame.MasterGameID,
                x.PickupBid.MasterGame.GameName,
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
        var testData = _results[ActionProcessingSetName].Results.FailedBids.Select(x => new
            {
                x.PickupBid.BidID,
                x.PickupBid.Publisher.PublisherID,
                x.PickupBid.MasterGame.MasterGameID,
                x.PickupBid.MasterGame.GameName,
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
        var testData = _results[ActionProcessingSetName].Results.SuccessDrops.Select(x => new
            {
                x.DropRequestID,
                x.Publisher.PublisherID,
                x.MasterGame.MasterGameID,
                x.MasterGame.GameName
            })
            .OrderBy(x => x.DropRequestID)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task FailedDropsTest()
    {
        var testData = _results[ActionProcessingSetName].Results.FailedDrops.Select(x => new
            {
                x.DropRequestID,
                x.Publisher.PublisherID,
                x.MasterGame.MasterGameID,
                x.MasterGame.GameName
            })
            .OrderBy(x => x.DropRequestID)
            .ToList();

        return Verifier.Verify(testData);
    }

    [Test]
    public Task AddedPublisherGamesTest()
    {
        var testData = _results[ActionProcessingSetName].Results.AddedPublisherGames.Select(x => new
            {
                x.PublisherID,
                x.MasterGame!.MasterGame.MasterGameID,
                x.MasterGame!.MasterGame.GameName,
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
        var testData = _results[ActionProcessingSetName].Results.RemovedPublisherGames.Select(x => new
            {
                x.PublisherGame.PublisherID,
                x.PublisherGame.MasterGame!.MasterGame.MasterGameID,
                x.PublisherGame.MasterGame!.MasterGame.GameName,
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
        var testData = _results[ActionProcessingSetName].Results.LeagueActions.Select(x => new
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
        Assert.That(_results[ActionProcessingSetName].SpecialAuctionsProcessed, Is.Empty);
    }
}
