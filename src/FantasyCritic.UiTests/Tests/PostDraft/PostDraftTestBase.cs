using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.UiTests.Tests.Draft;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.PostDraft;

/// <summary>
/// Leagues whose draft is over, with open roster and counter pick slots, bids, drops and trades all on.
/// </summary>
public abstract class PostDraftTestBase : DraftTestBase
{
    protected static LeagueScenario PostDraftScenario(int players) => new()
    {
        Name = $"UiPostDraft{players}",
        PlayerCount = players,
        StandardGames = 4,
        GamesToDraft = 2,
        CounterPicks = 2,
        CounterPicksToDraft = 1,
        EnableBids = true,
        WillReleaseDroppableGames = 2,
        WillNotReleaseDroppableGames = 2,
        DropOnlyDraftGames = true,
        CounterPicksBlockDrops = false,
        TradingSystem = "Standard",
    };

    protected async Task<(LeagueFixture League, Guid ManagerUserID)> NewDraftedLeagueAsync(int players = 2)
    {
        var (league, managerUserID) = await NewLeagueAsync(PostDraftScenario(players), startDraft: true);
        await league.DraftToCompletionAsync();
        return (league, managerUserID);
    }

    protected static string LeaguePath(LeagueFixture league) => $"/league/{league.LeagueID}/{league.Year}";

    /// <summary>
    /// Unreleased games nobody in the league has, which the publisher can bid on or watch.
    /// </summary>
    protected static async Task<IReadOnlyList<MasterGameYearViewModel>> AvailableGamesAsync(LeagueFixture league, TestPublisher publisher, int count)
    {
        var available = await publisher.Session.League.TopAvailableGamesAsync(league.Year, league.LeagueID, publisher.PublisherID, null);
        return available.Where(game => game.IsAvailable && !game.Taken && !game.IsReleased).Select(game => game.MasterGame).Take(count).ToList();
    }

    protected static async Task<PublisherViewModel> PublisherAsync(LeagueFixture league, TestPublisher publisher) =>
        (await league.GetLeagueYearAsync()).Publishers.Single(p => p.PublisherID == publisher.PublisherID);

    /// <summary>
    /// The nameless confirm dialog nested in a modal, found by its question. If the library renders it inside the outer
    /// dialog rather than at the end of the body, both match, and the inner one comes last.
    /// </summary>
    protected static ILocator ConfirmDialog(IPage page, string question) => page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = question }).Last;

    protected static async Task ConfirmAsync(IPage page, string question)
    {
        var dialog = ConfirmDialog(page, question);
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
    }
}
