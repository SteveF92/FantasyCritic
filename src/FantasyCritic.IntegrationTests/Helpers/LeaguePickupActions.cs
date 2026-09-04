using System;
using System.Text.Json;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.Lib;

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

    public static Task<PickupBidResultViewModel> TryPlaceBidAsync(
        TestPublisher publisher,
        Guid masterGameID,
        int bidAmount,
        bool counterPick,
        Guid? conditionalDropPublisherGameID = null)
    {
        return publisher.Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = publisher.PublisherID,
            MasterGameID = masterGameID,
            CounterPick = counterPick,
            BidAmount = bidAmount,
            AllowIneligibleSlot = false,
            ConditionalDropPublisherGameID = conditionalDropPublisherGameID,
        });
    }

    public static async Task<PickupBidResultViewModel> TryEditPickupBidAsync(
        TestPublisher publisher,
        Guid bidID,
        int bidAmount,
        Guid? conditionalDropPublisherGameID,
        bool allowIneligibleSlot = false)
    {
        var request = new PickupBidEditRequest
        {
            PublisherID = publisher.PublisherID,
            BidID = bidID,
            BidAmount = bidAmount,
            AllowIneligibleSlot = allowIneligibleSlot,
            ConditionalDropPublisherGameID = conditionalDropPublisherGameID,
        };

        // EditPickupBidAsync returns Task (no typed body) because the controller lacks
        // ProducesResponseType<PickupBidResultViewModel>; POST directly to read Success/Errors.
        var response = await publisher.Session.PostJsonAsync("api/League/EditPickupBid", request);
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<PickupBidResultViewModel>(body, FantasyCriticJsonOptions.Default)
                ?? throw new InvalidOperationException("EditPickupBid returned null body.");
        }

        return new PickupBidResultViewModel
        {
            Success = false,
            Errors = [body],
        };
    }

    public static async Task<(bool Success, string? Error)> TryDeleteDropAsync(
        TestPublisher publisher,
        Guid dropRequestID)
    {
        try
        {
            await publisher.Session.League.DeleteDropRequestAsync(new DropGameRequestDeleteRequest
            {
                PublisherID = publisher.PublisherID,
                DropRequestID = dropRequestID,
            });
            return (true, null);
        }
        catch (ApiException ex)
        {
            return (false, ex.Response);
        }
    }
}
