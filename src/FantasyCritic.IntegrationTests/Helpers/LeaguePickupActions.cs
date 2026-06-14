using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Shared helpers for placing pickup bids and drop requests in league action tests.
/// </summary>
internal static class LeaguePickupActions
{
    public static async Task PlaceBidAsync(
        TestPublisher publisher,
        Guid masterGameID,
        int bidAmount,
        bool counterPick,
        Guid? conditionalDropPublisherGameID = null)
    {
        var result = await publisher.Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = publisher.PublisherID,
            MasterGameID = masterGameID,
            CounterPick = counterPick,
            BidAmount = bidAmount,
            AllowIneligibleSlot = false,
            ConditionalDropPublisherGameID = conditionalDropPublisherGameID,
        });

        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakePickupBid failed for publisher {publisher.PublisherID}, game {masterGameID}, " +
                $"amount {bidAmount}, counterPick {counterPick}, conditionalDrop {conditionalDropPublisherGameID}. " +
                $"Errors: {errors}");
        }
    }

    public static async Task PlaceDropAsync(TestPublisher publisher, Guid publisherGameID)
    {
        var result = await publisher.Session.League.MakeDropRequestAsync(new DropGameRequestRequest
        {
            PublisherID = publisher.PublisherID,
            PublisherGameID = publisherGameID,
        });

        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakeDropRequest failed for publisher {publisher.PublisherID}, publisherGame {publisherGameID}. " +
                $"Errors: {errors}");
        }
    }
}
